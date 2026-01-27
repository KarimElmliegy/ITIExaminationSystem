// Global courses object with all course data
const coursesData = {};

var currentStudentId = @Model.StudentId;

// Initialize when page loads
document.addEventListener('DOMContentLoaded', function () {
    console.log('Page loaded, initializing...');
    console.log('Course cards in DOM:', document.querySelectorAll('.course-card').length);
    console.log('Courses data loaded:', coursesData);

    setupSearch();
    setupModalEventListeners();
    setupSearchInputAnimations();

    // Start countdown timers
    startCountdownTimers();
});

/**
 * Setup search functionality
 */
function setupSearch() {
    const searchInput = document.getElementById('courseSearch');
    const coursesGrid = document.getElementById('coursesGrid');

    if (!searchInput) {
        console.error('Search input not found');
        return;
    }

    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase().trim();
        const allCards = document.querySelectorAll('.course-card');
        let visibleCount = 0;

        allCards.forEach(card => {
            const title = card.dataset.courseTitle || '';
            const code = card.dataset.courseCode || '';
            const instructor = card.dataset.courseInstructor || '';

            const matches = title.includes(searchTerm) ||
                code.includes(searchTerm) ||
                instructor.includes(searchTerm);

            if (matches || searchTerm === '') {
                card.style.display = 'flex';
                visibleCount++;

                // Add highlight animation for matches
                if (searchTerm !== '') {
                    card.style.animation = 'courseCardAppear 0.5s ease forwards';
                }
            } else {
                card.style.display = 'none';
            }
        });

        updateSearchResults(visibleCount);
    });
}

/**
 * Update search results count
 */
function updateSearchResults(count) {
    const resultsElement = document.getElementById('searchResults');
    const totalCourses = Object.keys(coursesData).length;

    if (!resultsElement) return;

    // reset state
    resultsElement.classList.remove('found', 'empty');

    if (count === totalCourses) {
        resultsElement.textContent = `Showing all ${totalCourses} courses`;
    }
    else if (count === 0) {
        resultsElement.textContent = 'No courses found. Try a different search.';
    }
    else {
        resultsElement.textContent = `Found ${count} of ${totalCourses} courses`;
    }
}

/**
 * Open course modal with course details
 */
function openCourseModal(courseId) {
    console.log('Opening modal for course ID:', courseId);
    const course = coursesData[courseId];

    if (!course) {
        console.error('Course not found with ID:', courseId);
        console.log('Available courses:', coursesData);
        alert('Course not found!');
        return;
    }

    // Add shake animation to clicked card
    const clickedCard = document.querySelector(`[data-course-id="${courseId}"]`);
    if (clickedCard) {
        clickedCard.classList.add('shake');
        setTimeout(() => clickedCard.classList.remove('shake'), 500);
    }

    // Set modal title
    document.getElementById('modalTitle').textContent = `${course.title} (${course.code})`;

    // Generate content HTML
    const contentHtml = generateContentHtml(course);
    const examsHtml = generateExamsHtml(course);

    // Populate modal body
    const modalBody = document.getElementById('modalBody');
    modalBody.innerHTML = `
        <div class="modal-tabs">
            <button class="tab-btn active" data-tab="content" onclick="switchTab('content')">
                <i class="fas fa-list"></i> Course Overview
            </button>
            <button class="tab-btn" data-tab="examinations" onclick="switchTab('examinations')">
                <i class="fas fa-file-alt"></i> Examinations
            </button>
        </div>
        <div class="tab-content">
            <div id="content-tab" class="tab-pane active">
                ${contentHtml}
            </div>
            <div id="examinations-tab" class="tab-pane">
                ${examsHtml}
            </div>
        </div>
    `;

    // Show modal
    document.getElementById('courseModal').classList.add('active');

    // Restart countdown timers after modal content is loaded
    setTimeout(() => startCountdownTimers(), 100);
}

/**
 * Generate course content HTML
 */
function generateContentHtml(course) {
    let html = '<div class="content-section">';

    // Course Summary Stats
    html += `
        <h3 class="content-section-title"><i class="fas fa-chart-line"></i> Course Summary</h3>
        <div class="summary-stats">
            <div class="summary-stat">
                <div class="summary-value">${course.modules || 0}</div>
                <div class="summary-label">Modules</div>
            </div>
            <div class="summary-stat">
                <div class="summary-value">${course.exams || 0}</div>
                <div class="summary-label">Exams</div>
            </div>
            <div class="summary-stat">
                <div class="summary-value">${course.completed || 0}</div>
                <div class="summary-label">Completed</div>
            </div>
            <div class="summary-stat">
                <div class="summary-value">${(course.exams || 0) - (course.completed || 0)}</div>
                <div class="summary-label">Upcoming</div>
            </div>
        </div>
    `;

    // Course Description (if available)
    if (course.description && course.description.trim() !== '') {
        html += `
            <h3 class="content-section-title"><i class="fas fa-info-circle"></i> Course Description</h3>
            <div class="quick-info">
                <div class="info-points">
                    <div class="info-point">
                        <i class="fas fa-circle"></i>
                        <div class="info-point-text">${escapeHtml(course.description)}</div>
                    </div>
                </div>
            </div>
        `;
    }

    // Course Topics/Modules
    if (course.topics && course.topics.length > 0) {
        html += '<h3 class="content-section-title"><i class="fas fa-bookmark"></i> Course Modules</h3>';
        html += '<div class="headlines-list">';

        course.topics.forEach((topic, idx) => {
            html += `
                <div class="headline-item">
                    <div class="headline-number">${idx + 1}</div>
                    <div class="headline-text">${escapeHtml(topic || 'Topic ' + (idx + 1))}</div>
                </div>
            `;
        });

        html += '</div>';
    } else {
        html += `
            <h3 class="content-section-title"><i class="fas fa-bookmark"></i> Course Modules</h3>
            <div class="empty-state">
                <i class="fas fa-bookmark"></i>
                <p>No modules available for this course yet.</p>
            </div>
        `;
    }

    html += '</div>';
    return html;
}

/**
 * Generate exams HTML
 */
function generateExamsHtml(course) {
    if (!course.examList || course.examList.length === 0) {
        return `
            <div class="empty-state">
                <i class="fas fa-file-alt"></i>
                <h3>No exams available</h3>
                <p>There are no exams scheduled for this course yet.</p>
            </div>
        `;
    }

    const completedExams = course.examList.filter(e => e.isCompleted);
    // Scheduled includes Expired ones that aren't completed yet
    const scheduledExams = course.examList.filter(e => !e.isCompleted);

    let html = "";

    // 1. Completed Exams Section
    if (completedExams.length > 0) {
        html += `
            <div class="exam-section">
                <h3 class="exam-section-title">
                    <i class="fas fa-check-circle"></i> Completed Exams
                </h3>
        `;
        completedExams.forEach((exam, index) => {
            html += renderExamCard(exam, index, 'completed');
        });
        html += "</div>";
    }

    // 2. Scheduled / Expired Exams Section
    if (scheduledExams.length > 0) {
        html += `
            <div class="exam-section">
                <h3 class="exam-section-title">
                    <i class="fas fa-calendar-check"></i> Scheduled Exams
                </h3>
        `;
        scheduledExams.forEach((exam, index) => {
            html += renderExamCard(exam, index, 'scheduled');
        });
        html += "</div>";
    }

    return html;
}

/**
 * Renders a single exam card with live countdown timer
 */
function renderExamCard(exam, index, sectionType) {
    // Handle C# serialization case (PascalCase vs camelCase)
    const isExpired = exam.isExpired || exam.IsExpired;
    const isAvailable = exam.available || exam.Available;
    const isCompleted = sectionType === 'completed' || exam.isCompleted || exam.IsCompleted;

    // Get the full start date for countdown
    const fullStartDate = exam.fullStartDate || exam.FullStartDate;

    const examDate = formatExamDate(exam.date);
    let statusBadge = "";
    let actionSection = "";
    let cardClass = "exam-item";

    // ============================================================
    // 1. COMPLETED EXAM (Finished)
    // ============================================================
    if (isCompleted) {
        cardClass += " card-completed";
        statusBadge = `
            <span class="exam-status" style="background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb;">
                <i class="fas fa-check-circle"></i> Not Available Yet
            </span>`;

        actionSection = `
            <button class="exam-btn" style="
                background-color: #6c757d; 
                cursor: not-allowed;
                opacity: 0.7;
            " disabled>
                <i class="fas fa-lock"></i> Coming Soon
            </button>`;
    }
    // ============================================================
    // 2. EXPIRED EXAM (Not Entered & Time Passed)
    // ============================================================
    else if (isExpired) {
        cardClass += " card-expired";
        statusBadge = `
            <span class="exam-status" style="background-color: #ffebee; color: #c62828; border: 1px solid #ffcdd2;">
                <i class="fas fa-times-circle"></i> Expired
            </span>`;

        actionSection = `
            <button class="exam-btn" style="background-color: #95a5a6; cursor: not-allowed; opacity: 0.8;" disabled>
                <i class="fas fa-ban"></i> Expired
            </button>`;
    }
    // ============================================================
    // 3. AVAILABLE EXAM (Active & Ready to Start)
    // ============================================================
    else if (isAvailable) {
        cardClass += " card-active";
        statusBadge = `
            <span class="exam-status" style="background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb;">
                <i class="fas fa-unlock"></i> Available Now
            </span>`;

        actionSection = `
            <a href="/Exam/Start?examId=${exam.id}&studentId=${currentStudentId}" 
               class="exam-btn" 
               style="
                   background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                   box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
                   animation: pulse 2s infinite;
               ">
                <i class="fas fa-play-circle"></i> Start Exam
            </a>`;
    }
    // ============================================================
    // 4. UPCOMING EXAM (Coming Soon - Show Live Countdown)
    // ============================================================
    else {
        statusBadge = `
            <span class="exam-status" style="background-color: #fff3cd; color: #856404; border: 1px solid #ffeaa7;">
                <i class="fas fa-hourglass-half"></i> Not Available Yet
            </span>`;

        if (fullStartDate) {
            // Show countdown timer + Coming Soon button side by side
            actionSection = `
                <div id="exam-action-${exam.id}" style="display: flex; align-items: center; gap: 12px; flex-wrap: wrap;">
                    <div id="countdown-${exam.id}" 
                         class="countdown-timer" 
                         data-start-time="${fullStartDate}" 
                         data-exam-id="${exam.id}"
                         style="
                             background: linear-gradient(135deg, #f39c12, #e67e22);
                             color: white;
                             padding: 10px 18px;
                             border-radius: 8px;
                             font-weight: 600;
                             font-size: 0.95rem;
                             box-shadow: 0 2px 10px rgba(243, 156, 18, 0.3);
                             display: inline-flex;
                             align-items: center;
                             gap: 8px;
                             min-width: 140px;
                             justify-content: center;
                         ">
                        <i class="fas fa-clock"></i>
                        <span class="countdown-text">Loading...</span>
                    </div>
                    <button class="exam-btn" style="
                        background-color: #6c757d; 
                        cursor: not-allowed;
                        opacity: 0.7;
                    " disabled>
                        <i class="fas fa-lock"></i> Coming Soon
                    </button>
                </div>`;
        } else {
            // Fallback if no start date/time
            actionSection = `
                <button class="exam-btn" style="background-color: #6c757d; cursor: not-allowed; opacity: 0.7;" disabled>
                    <i class="fas fa-lock"></i> Coming Soon
                </button>`;
        }
    }

    return `
        <div class="${cardClass}" style="
            animation-delay: ${index * 0.1}s; 
            border-left: ${isAvailable && !isCompleted ? '4px solid #667eea' : '3px solid rgba(255,255,255,0.1)'};
        ">
            <div class="exam-item-header">
                <div class="exam-name">${escapeHtml(exam.name || `Exam ${exam.id}`)}</div>
                <div class="exam-type">${escapeHtml(exam.type || 'Exam')}</div>
            </div>
            <div class="exam-details">
                <div class="exam-detail-item"><i class="fas fa-calendar-alt"></i> ${examDate}</div>
                <div class="exam-detail-item"><i class="fas fa-stopwatch"></i> ${exam.duration || 'N/A'} minutes</div>
                <div class="exam-detail-item"><i class="fas fa-list-ol"></i> ${exam.questionCount || 'N/A'} Questions</div>
            </div>
            <div class="exam-footer" style="
                display: flex; 
                justify-content: space-between; 
                align-items: center; 
                margin-top: 20px; 
                flex-wrap: wrap; 
                gap: 12px;
            ">
                ${statusBadge}
                ${actionSection}
            </div>
        </div>
    `;
}

/**
 * Start countdown timers for all upcoming exams
 */
function startCountdownTimers() {
    // Update every second
    setInterval(updateAllCountdowns, 1000);
    // Run immediately once
    updateAllCountdowns();
}

/**
 * Update all countdown timers
 */
function updateAllCountdowns() {
    const countdownElements = document.querySelectorAll('.countdown-timer[data-start-time]');
    const now = new Date().getTime();

    countdownElements.forEach(element => {
        const startTimeStr = element.getAttribute('data-start-time');
        const examId = element.getAttribute('data-exam-id');

        if (!startTimeStr || !examId) return;

        try {
            const startTime = new Date(startTimeStr).getTime();
            const distance = startTime - now;

            if (distance < 0) {
                // ✅ TIME IS UP! TRANSFORM THE ENTIRE SECTION
                const actionSection = document.getElementById(`exam-action-${examId}`);
                if (actionSection) {
                    actionSection.innerHTML = `
                        <span class="exam-status" style="
                            background-color: #d4edda; 
                            color: #155724; 
                            border: 1px solid #c3e6cb;
                            animation: pulse 2s infinite;
                        ">
                            <i class="fas fa-unlock"></i> Available Now
                        </span>
                        <a href="/Exam/Start?examId=${examId}&studentId=${currentStudentId}" 
                           class="exam-btn" 
                           style="
                               background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                               box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
                               animation: pulse 2s infinite;
                           ">
                            <i class="fas fa-play-circle"></i> Start Exam
                        </a>`;

                    // Update the status badge too
                    const parentCard = actionSection.closest('.exam-item');
                    if (parentCard) {
                        const statusBadge = parentCard.querySelector('.exam-status');
                        if (statusBadge && statusBadge.textContent.includes('Not Available Yet')) {
                            statusBadge.innerHTML = `<i class="fas fa-unlock"></i> Available Now`;
                            statusBadge.style.backgroundColor = '#d4edda';
                            statusBadge.style.color = '#155724';
                            statusBadge.style.borderColor = '#c3e6cb';
                        }
                    }
                }
            } else {
                // 🕐 STILL COUNTING DOWN
                const days = Math.floor(distance / (1000 * 60 * 60 * 24));
                const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((distance % (1000 * 60)) / 1000);

                let timeString = '';
                if (days > 0) {
                    timeString = `${days}d ${hours}h ${minutes}m`;
                } else if (hours > 0) {
                    timeString = `${hours}h ${minutes}m ${seconds}s`;
                } else if (minutes > 0) {
                    timeString = `${minutes}m ${seconds}s`;
                } else {
                    timeString = `${seconds}s`;
                }

                const textSpan = element.querySelector('.countdown-text');
                if (textSpan) {
                    textSpan.textContent = timeString;
                }
            }
        } catch (error) {
            console.error('Error updating countdown for exam', examId, ':', error);
        }
    });
}

/**
 * Switch between modal tabs
 */
function switchTab(tabName) {
    // Update tab buttons
    document.querySelectorAll(".tab-btn").forEach((btn) => {
        btn.classList.remove("active");
        if (btn.dataset.tab === tabName) {
            btn.classList.add("active");
        }
    });

    // Update tab panes
    document.querySelectorAll(".tab-pane").forEach((pane) => {
        pane.classList.remove("active");
    });

    const targetPane = document.getElementById(`${tabName}-tab`);
    if (targetPane) {
        targetPane.classList.add("active");
    }
}

/**
 * Close the modal
 */
function closeModal() {
    const modal = document.getElementById("courseModal");
    if (modal) {
        modal.classList.remove("active");
    }
}

/**
 * Scroll to top of page
 */
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: "smooth" });
}

/**
 * Setup modal event listeners
 */
function setupModalEventListeners() {
    // Close modal when clicking outside
    window.onclick = function (event) {
        const modal = document.getElementById('courseModal');
        if (event.target === modal) {
            closeModal();
        }
    };

    // Close modal with Escape key
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeModal();
        }
    });
}

/**
 * Setup search input animations
 */
function setupSearchInputAnimations() {
    const searchInput = document.getElementById('courseSearch');

    if (!searchInput) return;

    searchInput.addEventListener('focus', function () {
        this.parentElement.style.transform = 'scale(1.02)';
    });

    searchInput.addEventListener('blur', function () {
        this.parentElement.style.transform = 'scale(1)';
    });
}

/**
 * Format exam date
 */
function formatExamDate(date) {
    if (!date) return 'Not scheduled';

    try {
        const dateObj = new Date(date);
        return dateObj.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    } catch (error) {
        return 'Not scheduled';
    }
}

/**
 * Escape HTML to prevent XSS
 */
function escapeHtml(text) {
    if (!text) return '';

    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };

    return text.replace(/[&<>"']/g, m => map[m]);
}

// ✅ FORCE RELOAD ON PAGE NAVIGATION (Fixes Back Button Issue)
window.addEventListener("pageshow", function (event) {
    var historyTraversal = event.persisted ||
        (typeof window.performance != "undefined" &&
            window.performance.navigation.type === 2);
    if (historyTraversal) {
        // Page was restored from cache (user clicked Back button)
        // Force a reload to get fresh Exam Status
        window.location.reload();
    }
});

// Add pulse animation CSS
const style = document.createElement('style');
style.innerHTML = `
@keyframes pulse {
    0% { 
        transform: scale(1); 
        box-shadow: 0 0 0 0 rgba(102, 126, 234, 0.7); 
    }
    70% { 
        transform: scale(1.05); 
        box-shadow: 0 0 0 10px rgba(102, 126, 234, 0); 
    }
    100% { 
        transform: scale(1); 
        box-shadow: 0 0 0 0 rgba(102, 126, 234, 0); 
    }
}

.countdown-timer {
    transition: all 0.3s ease;
}

.countdown-timer:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(243, 156, 18, 0.4);
}
`;
document.head.appendChild(style);

// Add smooth scroll behavior
document.documentElement.style.scrollBehavior = 'smooth';


