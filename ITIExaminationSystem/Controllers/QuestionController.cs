using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.Models.DTOs.Questions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITIExaminationSystem.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ExaminationSystemContext _db;

        public QuestionController(ExaminationSystemContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /Question/Index
        // Renders the question management page; passes lookup data for
        // the filter dropdowns via ViewBag.
        // ─────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.Courses = await _db.Courses.OrderBy(c => c.CourseName).ToListAsync();
            ViewBag.Instructors = await _db.Instructors
                                           .Include(i => i.User)
                                           .OrderBy(i => i.User.UserName)
                                           .ToListAsync();
            return View();
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /Question/GetAll?search=...&courseId=...&type=...&instructorId=...
        // Returns a JSON array of QuestionListDto (used by the client-side
        // search / filter logic without a full page reload).
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string search = "",
            int? courseId = null,
            string type = "",
            int? instructorId = null)
        {
            var query = _db.Questions
                           .Include(q => q.Course)
                           .Include(q => q.Instructor).ThenInclude(i => i.User)
                           .Include(q => q.Choices)
                           .AsQueryable();

            // ── text search ──────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(q => q.QuestionText.Contains(search));

            // ── filter by course ─────────────────────────────────────────
            if (courseId.HasValue)
                query = query.Where(q => q.CourseId == courseId.Value);

            // ── filter by question type ──────────────────────────────────
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(q => q.QuestionType == type);

            // ── filter by instructor ─────────────────────────────────────
            if (instructorId.HasValue)
                query = query.Where(q => q.InstructorId == instructorId.Value);

            var questions = await query
                .OrderBy(q => q.QuestionId)
                .Select(q => new QuestionListDto
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    CourseId = q.CourseId ?? 0,
                    CourseName = q.Course.CourseName,
                    InstructorId = q.InstructorId ?? 0,
                    InstructorName = q.Instructor.User.UserName,
                    ChoiceCount = q.Choices.Count
                })
                .ToListAsync();

            return Json(questions);
        }

        // ─────────────────────────────────────────────────────────────────
        // GET /Question/GetDetail/{id}
        // Returns full detail (including choices + correct flags) as JSON.
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var question = await _db.Questions
                .Include(q => q.Course)
                .Include(q => q.Instructor).ThenInclude(i => i.User)
                .Include(q => q.Choices)
                .Include(q => q.ChoicesNavigation)   // correct choices junction
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                return NotFound(new { message = "Question not found" });

            var correctChoiceIds = question.ChoicesNavigation.Select(c => c.ChoiceId).ToHashSet();

            var dto = new QuestionDetailDto
            {
                QuestionId = question.QuestionId,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                CorrectTf = question.CorrectTf,
                CourseId = question.CourseId ?? 0 ,
                CourseName = question.Course?.CourseName,
                InstructorId = question.InstructorId ?? 0,
                InstructorName = question.Instructor?.User?.UserName,
                Choices = question.Choices.Select(c => new ChoiceDetailDto
                {
                    ChoiceId = c.ChoiceId,
                    ChoiceText = c.ChoiceText,
                    IsCorrect = correctChoiceIds.Contains(c.ChoiceId)
                }).ToList()
            };

            return Json(dto);
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /Question/Update
        // Saves edits to an existing question (text, type, choices, correct
        // answers). Handles both MCQ and TF question types.
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateQuestionDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid payload" });

            var question = await _db.Questions
                .Include(q => q.Choices)
                .Include(q => q.ChoicesNavigation)
                .FirstOrDefaultAsync(q => q.QuestionId == dto.QuestionId);

            if (question == null)
                return NotFound(new { success = false, message = "Question not found" });

            // ── basic fields ─────────────────────────────────────────────
            question.QuestionText = dto.QuestionText?.Trim();
            question.QuestionType = dto.QuestionType;
            question.CourseId = dto.CourseId;

            if (dto.QuestionType == "TF")
            {
                question.CorrectTf = dto.CorrectTf;

                // remove all MCQ choices and correct-choice links
                var allChoiceIds = question.Choices.Select(c => c.ChoiceId).ToList();
                question.ChoicesNavigation.Clear();
                _db.Choices.RemoveRange(question.Choices);
            }
            else  // MCQ
            {
                question.CorrectTf = null;

                // ── handle existing choices ──────────────────────────────
                var existingChoiceIds = question.Choices.Select(c => c.ChoiceId).ToHashSet();

                // choices to delete
                var toDelete = dto.Choices
                    .Where(c => c.IsDeleted && c.ChoiceId.HasValue)
                    .Select(c => c.ChoiceId.Value)
                    .ToList();

                foreach (var cid in toDelete)
                {
                    var ch = question.Choices.FirstOrDefault(c => c.ChoiceId == cid);
                    if (ch != null)
                    {
                        // remove from correct-choices junction first
                        var correct = question.ChoicesNavigation.FirstOrDefault(c => c.ChoiceId == cid);
                        if (correct != null) question.ChoicesNavigation.Remove(correct);
                        _db.Choices.Remove(ch);
                    }
                }

                // update or add choices
                question.ChoicesNavigation.Clear();   // rebuild correct-choices list

                foreach (var choiceDto in dto.Choices.Where(c => !c.IsDeleted))
                {
                    Choice choiceEntity;

                    if (choiceDto.ChoiceId.HasValue && existingChoiceIds.Contains(choiceDto.ChoiceId.Value))
                    {
                        // update existing
                        choiceEntity = question.Choices.First(c => c.ChoiceId == choiceDto.ChoiceId.Value);
                        choiceEntity.ChoiceText = choiceDto.ChoiceText?.Trim();
                    }
                    else
                    {
                        // add new
                        choiceEntity = new Choice
                        {
                            ChoiceText = choiceDto.ChoiceText?.Trim(),
                            QuestionId = question.QuestionId
                        };
                        _db.Choices.Add(choiceEntity);
                        await _db.SaveChangesAsync();   // get the new ChoiceId
                    }

                    if (choiceDto.IsCorrect)
                        question.ChoicesNavigation.Add(choiceEntity);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Question updated successfully" });
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /Question/Delete/{id}
        // Removes a question and all related data (choices, correct choices,
        // exam links, answers).
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _db.Questions
                .Include(q => q.Choices)
                .Include(q => q.ChoicesNavigation)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null)
                return NotFound(new { success = false, message = "Question not found" });

            // remove related answers (cascade may not be set)
            _db.Answers.RemoveRange(question.Answers);

            // clear junctions first
            question.ChoicesNavigation.Clear();
            question.Exams.Clear();

            // remove choices
            _db.Choices.RemoveRange(question.Choices);

            // remove question
            _db.Questions.Remove(question);

            await _db.SaveChangesAsync();
            return Ok(new { success = true, message = "Question deleted successfully" });
        }
    }
}