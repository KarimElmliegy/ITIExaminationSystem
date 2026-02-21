// ==========================================
// INSTRUCTOR DASHBOARD - INSTRUCTOR-SPECIFIC FUNCTIONALITY
// Common modal functions moved to shared.js
// ==========================================

// Note: openModal, closeModal, createRippleEffect are now in shared.js

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

    // Close modal on overlay click (instructor-specific delete modals)
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

    // Close modal on Escape key (instructor-specific delete modals)
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

        // Handle edit buttons for courses
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
// OPEN EDIT COURSE MODAL
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
// HANDLE EDIT COURSE FORM SUBMISSION
// ==========================================
function handleEditCourse(event) {
    event.preventDefault();

    const idValue = document.getElementById('edit-course-id').value;

    if (!idValue) {
        alert("Course ID is missing");
        return;
    }

    const payload = {
        CourseId: Number(idValue),
        CourseName: document.getElementById('edit-course-name').value,
        Duration: Number(document.getElementById('edit-course-dur').value)
    };

    fetch('/Instructor/EditCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
        .then(r => r.text())
        .then(msg => {
            closeModal();
            window.location.reload();
        })
        .catch(err => {
            console.error(err);
            alert("Update failed");
        });
}

// ==========================================
// HANDLE ADD COURSE FORM SUBMISSION
// ==========================================
function handleAddCourse(event) {
    event.preventDefault();

    const courseName = document.getElementById('inp-course-name').value;
    const duration = document.getElementById('inp-course-dur').value;

    if (!courseName || !duration) {
        alert("Please fill in all fields");
        return;
    }

    const payload = {
        CourseName: courseName,
        Duration: Number(duration)
    };

    fetch('/Instructor/AddCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
        .then(r => r.text())
        .then(msg => {
            closeModal();
            window.location.reload();
        })
        .catch(err => {
            console.error(err);
            alert("Failed to add course");
        });
}

// ==========================================
// DELETE STUDENT MODAL FUNCTIONS
// ==========================================
function openDeleteModal(userId, userName, userEmail) {
    createDeleteModal();

    document.getElementById('delete-user-id').value = userId;
    const nameEl = document.getElementById('delete-user-name');
    const emailEl = document.getElementById('delete-user-email');

    if (nameEl) nameEl.textContent = userName || 'Unknown User';
    if (emailEl) emailEl.textContent = userEmail || '';

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
    if (document.getElementById('modal-delete-student')) return;

    const modalHTML = `
        <div id="modal-delete-student" class="modal-box delete-modal-container hidden" style="transition: all 0.3s ease;">
            <div class="icon-box">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                    <line x1="12" y1="9" x2="12" y2="13"></line>
                    <line x1="12" y1="17" x2="12.01" y2="17"></line>
                </svg>
            </div>
            <h2>Delete Student</h2>
            <p class="confirm-text">Are you sure you want to delete this student?</p>
            <span class="target-text" id="delete-user-name">Student Name</span>
            <p class="warning-details">This action cannot be undone. This will permanently delete the student account and remove all associated data from the system.</p>
            <input type="hidden" id="delete-user-id" />
            <div class="button-row">
                <button type="button" class="btn-cancel" onclick="cancelDelete()">Cancel</button>
                <button type="button" class="btn-delete-red" onclick="performDelete()">Delete Student</button>
            </div>
        </div>
    `;

    const overlay = document.getElementById('modal-overlay');
    if (overlay) {
        overlay.insertAdjacentHTML('afterend', modalHTML);
    }
}

function cancelDelete() {
    const modal = document.getElementById('modal-delete-student');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.95)';
        setTimeout(() => {
            modal.classList.add('hidden');
            overlay.classList.add('hidden');
            document.body.classList.remove('modal-open');
            currentModal = null;
        }, 300);
    }
}

function performDelete() {
    const userId = document.getElementById('delete-user-id').value;
    const deleteBtn = document.querySelector('#modal-delete-student .btn-delete-red');

    if (!deleteBtn) return;

    const originalContent = deleteBtn.innerHTML;

    if (!userId) {
        alert('Invalid user ID');
        return;
    }

    deleteBtn.classList.add('loading');
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = `<div class="spinner" style="display:inline-block; width:16px; height:16px; border:2px solid white; border-top-color:transparent; border-radius:50%; animation:spin 1s linear infinite;"></div> Deleting...`;

    fetch(`/Instructor/DeleteStudent?studentId=${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || 'Failed to delete student');
                });
            }
            return response.text();
        })
        .then(result => {
            cancelDelete();
            window.location.reload();
        })
        .catch(error => {
            console.error('Delete error:', error);
            alert('Error: ' + error.message);
            deleteBtn.classList.remove('loading');
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalContent;
        });
}

// ==========================================
// DELETE COURSE MODAL FUNCTIONS
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
    if (document.getElementById('modal-delete-course')) return;

    const modalHTML = `
        <div id="modal-delete-course" class="modal-box delete-modal-container hidden" style="transition: all 0.3s ease;">
            <div class="icon-box">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                    <line x1="12" y1="9" x2="12" y2="13"></line>
                    <line x1="12" y1="17" x2="12.01" y2="17"></line>
                </svg>
            </div>
            <h2>Delete Course</h2>
            <p class="confirm-text">Are you sure you want to delete this course?</p>
            <span class="target-text" id="delete-course-name">Course Name</span>
            <p class="warning-details">This action cannot be undone. This will permanently delete the course and remove all associated exams and questions from the system.</p>
            <input type="hidden" id="delete-course-id" />
            <div class="button-row">
                <button type="button" class="btn-cancel" onclick="cancelDeleteCourse()">Cancel</button>
                <button type="button" class="btn-delete-red" onclick="performDeleteCourse()">Delete Course</button>
            </div>
        </div>
    `;

    const overlay = document.getElementById('modal-overlay');
    if (overlay) {
        overlay.insertAdjacentHTML('afterend', modalHTML);
    }
}

function cancelDeleteCourse() {
    const modal = document.getElementById('modal-delete-course');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.95)';
        setTimeout(() => {
            modal.classList.add('hidden');
            overlay.classList.add('hidden');
            document.body.classList.remove('modal-open');
            currentModal = null;
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
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text || 'Failed to delete course');
                });
            }
            return response.text();
        })
        .then(result => {
            cancelDeleteCourse();
            window.location.reload();
        })
        .catch(error => {
            console.error('Delete error:', error);
            alert('Error: ' + error.message);
            deleteBtn.classList.remove('loading');
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalContent;
        });
}
