// ==========================================
// GLOBAL STATE & CONFIGURATION
// ==========================================
let currentModal = null;
let currentlyEditingRow = null;

// DOM Elements
const modalOverlay = document.getElementById("modal-overlay");
const logoutBtn = document.getElementById("logoutBtn");

// ==========================================
// INITIALIZATION
// ==========================================
document.addEventListener("DOMContentLoaded", function () {
    // Initialize Lucide icons
    if (window.lucide) {
        lucide.createIcons();
    }

    // Setup navigation active states
    setupNavigation();

    // Setup table actions using event delegation
    setupTableActions();

    // Setup intake roller
    setupIntakeRoller();

    // Add ripple effects to buttons
    document.querySelectorAll('button:not(.btn-delete), .action-btn:not(.btn-delete)').forEach(button => {
        button.addEventListener('click', function (e) {
            createRippleEffect(this, e);
        });
    });

    // Close modal on overlay click
    if (modalOverlay) {
        modalOverlay.addEventListener('click', function (e) {
            if (e.target === this) {
                if (currentModal === 'modal-delete-student') {
                    cancelDelete();
                }
                else if (currentModal === 'modal-delete-course') {
                    cancelDeleteCourse();
                }
                else {
                    closeModal();
                }
            }
        });
    }

    // Close modal on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;

        if (currentModal === 'modal-delete-student') {
            cancelDelete();
        } else if (currentModal === 'modal-delete-course') {
            cancelDeleteCourse();
        } else {
            closeModal();
        }
    });
});

// ==========================================
// EVENT DELEGATION FOR ALL TABLE ACTIONS
// ==========================================
function setupTableActions() {
    document.addEventListener('click', function (e) {
        // Handle delete buttons
        const deleteBtn = e.target.closest('.btn-delete');
        if (deleteBtn) {
            e.preventDefault();
            e.stopImmediatePropagation();

            // 1. CHECK FOR COURSE DELETE
            if (deleteBtn.hasAttribute('data-course-id')) {
                const courseId = deleteBtn.getAttribute('data-course-id');
                const courseName = deleteBtn.getAttribute('data-course-name');
                if (courseId) {
                    openDeleteCourseModal(courseId, courseName);
                }
                return;
            }

            // 2. CHECK FOR STUDENT DELETE
            let userId = deleteBtn.getAttribute('data-user-id');
            let userName = deleteBtn.getAttribute('data-user-name');
            let userEmail = deleteBtn.getAttribute('data-user-email');

            if (!userId) {
                const row = deleteBtn.closest('tr');
                if (row) {
                    userId = row.getAttribute('data-user-id');
                    if (!userName) {
                        const nameCell = row.querySelector('td:nth-child(3)');
                        if (nameCell) userName = nameCell.innerText.trim();
                    }
                    if (!userEmail) {
                        const emailCell = row.querySelector('td:nth-child(4)');
                        if (emailCell) userEmail = emailCell.innerText.trim();
                    }
                }
            }

            if (userId) {
                openDeleteModal(userId, userName, userEmail);
            }
            return;
        }

        // ✅ Handle edit buttons for courses
        const editBtn = e.target.closest('.btn-edit');
        if (editBtn && editBtn.hasAttribute('data-course-id')) {
            e.preventDefault();
            e.stopPropagation();

            const courseId = editBtn.getAttribute('data-course-id');
            const courseName = editBtn.getAttribute('data-course-name');
            const instName = editBtn.getAttribute('data-inst-name');
            const instEmail = editBtn.getAttribute('data-inst-email');
            const duration = editBtn.getAttribute('data-duration');

            console.log('Edit Course:', { courseId, courseName, instName, instEmail, duration });

            // ✅ Call the separate edit modal function
            openEditCourseModal(courseId, courseName, duration);
            return;
        }
    });
}

// ==========================================
// NAVIGATION MANAGEMENT
// ==========================================
function setupNavigation() {
    const navLinks = document.querySelectorAll('.nav-link');
    const currentPath = window.location.pathname.toLowerCase().replace(/\/+$/, "");

    navLinks.forEach(link => {
        const rawHref = link.getAttribute('href');
        if (!rawHref) return;

        const linkPath = rawHref.toLowerCase().replace(/\/+$/, "");
        if (currentPath === linkPath) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    sessionStorage.removeItem('activeNav');
}

// ==========================================
// LOGOUT FUNCTIONALITY
// ==========================================
if (logoutBtn) {
    logoutBtn.addEventListener("click", (e) => {
        e.preventDefault();
        window.location.href = "/Home/LoginPage";
    });
}

// ==========================================
// MODAL MANAGEMENT
// ==========================================
function openModal(type) {
    const modalId = `modal-${type}`;
    currentModal = modalId;

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById(modalId);

    if (!overlay || !modal) return;

    document.body.classList.add('modal-open');
    overlay.classList.remove('hidden');

    // Reset form for ADD operations
    const form = modal.querySelector('form');
    if (form) form.reset();

    setTimeout(() => {
        modal.classList.remove('hidden');
        modal.classList.add('step-enter');

        const firstInput = modal.querySelector('input:not([type="hidden"]), select, textarea');
        if (firstInput) firstInput.focus();
    }, 50);

    // Set proper title for add modals
    const titleEl = modal.querySelector(".modal-title");
    if (titleEl && type === 'course') {
        titleEl.innerText = "Add New Course";
    }

    const submitBtn = modal.querySelector("button[type='submit']");
    if (submitBtn && type === 'course') {
        submitBtn.innerText = "Save Course";
    }
}

function closeModal() {
    if (!currentModal && !modalOverlay) return;

    document.body.classList.remove('modal-open');
    const overlay = document.getElementById('modal-overlay');
    const modal = currentModal ? document.getElementById(currentModal) : null;

    if (modal) {
        modal.classList.add('step-exit');
        setTimeout(() => {
            modal.classList.add('hidden');
            modal.classList.remove('step-enter', 'step-exit');
            overlay.classList.add('hidden');
            currentModal = null;
            currentlyEditingRow = null;

            // Reset all forms
            document.querySelectorAll("form").forEach((f) => f.reset());
        }, 400);
    } else {
        overlay.classList.add('hidden');
        document.querySelectorAll('.modal-box').forEach(modal => {
            modal.classList.add('hidden');
            modal.classList.remove('step-enter', 'step-exit');
        });
    }

    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');
    if (step1 && step2) {
        step1.classList.remove('hidden');
        step2.classList.add('hidden');
    }
}

// ==========================================
// ✅ OPEN EDIT COURSE MODAL
// ==========================================
function openEditCourseModal(courseId, courseName, duration) {
    document.getElementById('edit-course-id').value = courseId;
    document.getElementById('edit-course-name').value = courseName;
    document.getElementById('edit-course-dur').value = duration;

    currentModal = 'modal-edit-course';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-edit-course');

    overlay.classList.remove('hidden');
    modal.classList.remove('hidden');
}


// ==========================================
// ✅ HANDLE EDIT COURSE FORM SUBMISSION
// ==========================================
function handleEditCourse(event) {
    event.preventDefault();

    const courseId = document.getElementById('edit-course-id').value;
    const courseName = document.getElementById('edit-course-name').value;
    const duration = document.getElementById('edit-course-dur').value;

    const payload = {
        CourseId: parseInt(courseId),
        CourseName: courseName,
        Duration: duration
    };

    console.log('Updating course:', payload);

    fetch('/Instructor/EditCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to update course');
            }
            return response.text();
        })
        .then(message => {
            console.log(message);
            closeModal();
            window.location.reload();
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Failed to update course');
        });
}

// ==========================================
// ✅ HANDLE ADD COURSE FORM SUBMISSION
// ==========================================
function handleAddCourse(event) {
    event.preventDefault();

    const courseId = document.getElementById('inp-course-id').value;
    const courseName = document.getElementById('inp-course-name').value;
    const instName = document.getElementById('inp-course-inst').value;
    const instEmail = document.getElementById('inp-course-email').value;
    const duration = document.getElementById('inp-course-dur').value;

    const payload = {
        CourseId: courseId ? parseInt(courseId) : null,
        CourseName: courseName,
        InstructorName: instName,
        InstructorEmail: instEmail,
        Duration: duration
    };

    console.log('Adding course:', payload);

    fetch('/Instructor/AddCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(payload)
    })
    .then(response => {
        if (response.ok) return response.text();
        throw new Error('Failed to save course');
    })
    .then(message => {
        console.log(message);
        closeModal();
        window.location.reload();
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Failed to save course');
    });
}

// ==========================================
// EDIT STUDENT MODAL
// ==========================================
function openEditStudentModal(userId, userName, userEmail, branchId, trackId, intakeNumber) {
    const decodeHTML = (html) => {
        const txt = document.createElement('textarea');
        txt.innerHTML = html;
        return txt.value;
    };

    document.getElementById('edit-stud-id').value = userId;
    document.getElementById('edit-stud-name').value = decodeHTML(userName || '');
    document.getElementById('edit-stud-email').value = decodeHTML(userEmail || '');
    document.getElementById('edit-stud-branch').value = branchId || '';
    document.getElementById('edit-stud-track').value = trackId || '';
    document.getElementById('edit-stud-intake').value = intakeNumber || '';

    document.body.classList.add('modal-open');

    if (modalOverlay) {
        modalOverlay.classList.remove('hidden');
    }

    document.querySelectorAll('.modal-box').forEach(modal => modal.classList.add('hidden'));

    const editModal = document.getElementById('modal-edit-student');
    if (editModal) {
        editModal.classList.remove('hidden');
        currentModal = 'modal-edit-student';
    }
}

// ==========================================
// DELETE STUDENT MODAL FUNCTIONS
// ==========================================
function openDeleteModal(userId, userName, userEmail) {
    createDeleteModal();

    document.getElementById('delete-stud-id').value = userId;
    const nameEl = document.getElementById('delete-student-name');
    if (nameEl) nameEl.textContent = userName || 'Unknown Student';

    currentModal = 'modal-delete-student';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-delete-student');

    if (overlay && modal) {
        overlay.classList.remove('hidden');
        modal.classList.remove('hidden');

        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.95)';
        setTimeout(() => {
            modal.style.opacity = '1';
            modal.style.transform = 'scale(1)';
        }, 10);
    }
}

function createDeleteModal() {
    const overlay = document.getElementById('modal-overlay');
    const existing = document.getElementById('modal-delete-student');
    if (existing) existing.remove();

    const deleteModal = document.createElement('div');
    deleteModal.id = 'modal-delete-student';
    deleteModal.className = 'modal-box hidden delete-modal-container';

    deleteModal.innerHTML = `
        <div class="icon-box">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                <line x1="12" y1="9" x2="12" y2="13"></line>
                <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
        </div>
        <h2>Delete Student</h2>
        <p class="confirm-text">Are you sure you want to delete this student?</p>
        <span id="delete-student-name" class="target-text">Loading...</span>
        <p class="warning-details">
            This action cannot be undone. All student data, including grades and course enrollments, will be permanently deleted.
        </p>
        <input type="hidden" id="delete-stud-id" />
        <div class="button-row">
            <button type="button" class="btn-cancel" onclick="cancelDelete()">Cancel</button>
            <button type="button" class="btn-delete-red" onclick="performDeleteStudent()">Delete Student</button>
        </div>
    `;

    overlay.appendChild(deleteModal);
}

function cancelDelete() {
    const modal = document.getElementById('modal-delete-student');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'translateY(20px) scale(0.95)';
        modal.style.transition = 'all 0.4s cubic-bezier(0.4, 0, 0.2, 1)';
        overlay.style.opacity = '0';
        overlay.style.backdropFilter = 'blur(0px)';

        setTimeout(() => {
            modal.remove();
            overlay.classList.add('hidden');
            document.body.classList.remove('modal-open');
            currentModal = null;

            overlay.style.opacity = '';
            overlay.style.backdropFilter = '';
            overlay.style.transition = '';
        }, 400);
    }
}

function performDeleteStudent() {
    const userId = document.getElementById('delete-stud-id').value;
    const deleteBtn = document.querySelector('#modal-delete-student .btn-delete-red');

    if (!deleteBtn) {
        console.error("Delete button not found.");
        return;
    }

    const originalContent = deleteBtn.innerHTML;
    const modal = document.getElementById('modal-delete-student');

    if (!userId) {
        modal.style.animation = 'errorShake 0.5s ease';
        setTimeout(() => modal.style.animation = '', 500);
        alert('Invalid student ID');
        return;
    }

    deleteBtn.classList.add('loading');
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = `<div class="spinner" style="display:inline-block; width:16px; height:16px; border:2px solid white; border-top-color:transparent; border-radius:50%; animation:spin 1s linear infinite;"></div> Deleting...`;

    fetch(`/Instructor/DeleteStudent?userId=${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (response.ok) return response.text();
        throw new Error('Failed to delete student');
    })
    .then(text => {
        if (text === "Student deleted successfully") {
            deleteBtn.classList.remove('loading');
            deleteBtn.innerHTML = `<span>Deleted!</span>`;
            deleteBtn.style.background = '#10b981';

            const row = document.querySelector(`tr[data-user-id="${userId}"]`);
            if (row) {
                row.style.transition = 'all 0.5s ease';
                row.style.opacity = '0';
                row.style.transform = 'translateX(-50px)';

                setTimeout(() => {
                    row.remove();
                    const statEl = document.getElementById('stat-students');
                    if (statEl) statEl.innerText = parseInt(statEl.innerText) - 1;
                    setTimeout(() => cancelDelete(), 300);
                }, 500);
            } else {
                setTimeout(() => cancelDelete(), 1000);
            }
        } else {
            throw new Error('Failed to delete student');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        deleteBtn.classList.remove('loading');
        deleteBtn.disabled = false;
        deleteBtn.innerHTML = originalContent;
        alert('Deletion Failed: It seems this student has related data (answers/grades).');
    });
}

// ==========================================
// COURSE DELETE FUNCTIONS
// ==========================================
function openDeleteCourseModal(courseId, courseName) {
    createDeleteCourseModal();

    document.getElementById('delete-course-id').value = courseId;
    const nameEl = document.getElementById('delete-course-name');
    if (nameEl) nameEl.textContent = courseName || 'Unknown Course';

    currentModal = 'modal-delete-course';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-delete-course');

    if (overlay && modal) {
        overlay.classList.remove('hidden');
        modal.classList.remove('hidden');

        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.95)';
        setTimeout(() => {
            modal.style.opacity = '1';
            modal.style.transform = 'scale(1)';
        }, 10);
    }
}

function createDeleteCourseModal() {
    const overlay = document.getElementById('modal-overlay');
    const existing = document.getElementById('modal-delete-course');
    if (existing) existing.remove();

    const deleteModal = document.createElement('div');
    deleteModal.id = 'modal-delete-course';
    deleteModal.className = 'modal-box hidden delete-modal-container';

    deleteModal.innerHTML = `
        <div class="icon-box">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                <line x1="12" y1="9" x2="12" y2="13"></line>
                <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
        </div>
        <h2>Delete Course</h2>
        <p class="confirm-text">Are you sure you want to delete this course?</p>
        <span id="delete-course-name" class="target-text">Loading...</span>
        <p class="warning-details">
            This action cannot be undone. All course data, including questions and student answers, will be permanently deleted.
        </p>
        <input type="hidden" id="delete-course-id" />
        <div class="button-row">
            <button type="button" class="btn-cancel" onclick="cancelDeleteCourse()">Cancel</button>
            <button type="button" class="btn-delete-red" onclick="performDeleteCourse()">Delete Course</button>
        </div>
    `;

    overlay.appendChild(deleteModal);
}

function cancelDeleteCourse() {
    const modal = document.getElementById('modal-delete-course');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.9)';
        overlay.style.opacity = '0';

        setTimeout(() => {
            modal.remove();
            overlay.classList.add('hidden');
            document.body.classList.remove('modal-open');
            currentModal = null;

            overlay.style.opacity = '';
            overlay.style.transition = '';
        }, 300);
    }
}

function performDeleteCourse() {
    const courseId = document.getElementById('delete-course-id').value;
    const deleteBtn = document.querySelector('#modal-delete-course .btn-delete-red');

    if (!deleteBtn) return;

    const originalContent = deleteBtn.innerHTML;

    if (!courseId) {
        alert('Invalid course ID');
        return;
    }

    deleteBtn.classList.add('loading');
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = `<div class="spinner" style="display:inline-block; width:16px; height:16px; border:2px solid white; border-top-color:transparent; border-radius:50%; animation:spin 1s linear infinite;"></div> Deleting...`;

    fetch(`/Instructor/DeleteCourse?courseId=${courseId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (response.ok) return response.text();
        return response.text().then(text => { throw new Error(text) });
    })
    .then(text => {
        if (text === "Course deleted successfully") {
            const targetRow = document.querySelector(`button[data-course-id="${courseId}"]`)?.closest('tr');

            if (targetRow) {
                targetRow.style.transition = 'all 0.5s ease';
                targetRow.style.opacity = '0';
                setTimeout(() => {
                    targetRow.remove();
                    cancelDeleteCourse();
                    const statEl = document.getElementById('stat-courses');
                    if (statEl) statEl.innerText = parseInt(statEl.innerText) - 1;
                }, 500);
            } else {
                window.location.reload();
            }
        }
    })
    .catch(error => {
        console.error('Error:', error);
        deleteBtn.classList.remove('loading');
        deleteBtn.disabled = false;
        deleteBtn.innerHTML = originalContent;
        alert('Deletion Failed: This course has related data (exams/questions) that cannot be auto-deleted.');
    });
}

// ==========================================
// OTHER UTILITY FUNCTIONS
// ==========================================
function setupIntakeRoller() {
    const valueSpan = document.getElementById("rollerValue");
    const upBtn = document.querySelector(".roller-btn[data-step='1']");
    const downBtn = document.querySelector(".roller-btn[data-step='-1']");

    if (!valueSpan || !upBtn || !downBtn) return;

    const min = 1;
    const max = 46;
    let currentIntake = parseInt(valueSpan.textContent);

    upBtn.addEventListener("click", function (e) {
        e.preventDefault();
        let newIntake = currentIntake + 1;
        if (newIntake <= max) {
            window.location.href = `/Instructor/FilterByIntake?intake=${newIntake}`;
        }
    });

    downBtn.addEventListener("click", function (e) {
        e.preventDefault();
        let newIntake = currentIntake - 1;
        if (newIntake >= min) {
            window.location.href = `/Instructor/FilterByIntake?intake=${newIntake}`;
        }
    });
}

function showStep2() {
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');

    const inputs = step1.querySelectorAll('input[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.style.borderColor = '#ef4444';
            input.style.animation = 'shake 0.5s ease';
            isValid = false;

            setTimeout(() => {
                input.style.animation = '';
            }, 500);
        } else {
            input.style.borderColor = '#10b981';
        }
    });

    if (!isValid) {
        showNotification('Please fill in all required fields', 'error');
        return;
    }

    step1.classList.add('step-exit');

    setTimeout(() => {
        step1.classList.add('hidden');
        step2.classList.remove('hidden');
        step2.classList.add('step-enter');

        const firstInput = step2.querySelector('input');
        if (firstInput) firstInput.focus();
    }, 400);
}

function showStep1() {
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');

    step2.classList.add('step-exit');

    setTimeout(() => {
        step2.classList.add('hidden');
        step1.classList.remove('hidden');
        step1.classList.add('step-enter');
    }, 400);
}

function showNotification(message, type = 'info') {
    let container = document.querySelector('.notification-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'notification-container';
        document.body.appendChild(container);
    }

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 10px;">
            <i data-lucide="${type === 'success' ? 'check-circle' : type === 'error' ? 'alert-circle' : 'info'}"></i>
            <span>${message}</span>
        </div>
        <button onclick="this.parentElement.remove()" style="background: none; border: none; color: white; cursor: pointer; opacity: 0.7; transition: opacity 0.2s;" onmouseover="this.style.opacity='1'" onmouseout="this.style.opacity='0.7'">
            <i data-lucide="x"></i>
        </button>
    `;

    container.appendChild(notification);

    if (window.lucide) {
        lucide.createIcons();
    }

    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

function createRippleEffect(button, event) {
    const existingRipples = button.querySelectorAll('.ripple');
    existingRipples.forEach(ripple => ripple.remove());

    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.width = ripple.style.height = size + 'px';
    ripple.style.left = x + 'px';
    ripple.style.top = y + 'px';
    ripple.classList.add('ripple');

    button.appendChild(ripple);

    setTimeout(() => {
        if (ripple.parentNode === button) {
            ripple.remove();
        }
    }, 600);
}