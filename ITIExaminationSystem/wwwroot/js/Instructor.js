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
    document.querySelectorAll('button, .action-btn').forEach(button => {
        button.addEventListener('click', function (e) {
            createRippleEffect(this, e);
        });
    });

    // Close modal on overlay click
    if (modalOverlay) {
        modalOverlay.addEventListener('click', function (e) {
            if (e.target === this) {
                closeModal();
            }
        });
    }

    // Close modal on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && currentModal) {
            closeModal();
        }
    });
});

function handleAddCourse(e) {
    e.preventDefault();

    const courseId = document.getElementById('inp-course-id')?.value || null;
    const courseName = document.getElementById('inp-course-name').value;
    const instructorName = document.getElementById('inp-course-inst').value;
    const instructorEmail = document.getElementById('inp-course-email').value;
    const duration = document.getElementById('inp-course-dur').value;

    const data = {
        CourseId: courseId,
        CourseName: courseName,
        InstructorName: instructorName,
        InstructorEmail: instructorEmail,
        Duration: duration
    };


    fetch('/Instructor/AddCourse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(data)
    })
        .then(res => {
            if (!res.ok) throw new Error('Failed');
            return res.text();
        })
        .then(() => {
            closeModal();
            window.location.reload();
        })
        .catch(err => {
            console.error(err);
            alert('Error saving course');
        });
}

// ==========================================
// EVENT DELEGATION FOR ALL TABLE ACTIONS
// ==========================================
function setupTableActions() {
    document.addEventListener('click', function (e) {
        // Handle delete buttons
        const deleteBtn = e.target.closest('.btn-delete');
        if (deleteBtn) {
            e.preventDefault();
            e.stopPropagation();

            // Check if this is a course delete button (has data-course-id)
            if (deleteBtn.hasAttribute('data-course-id')) {
                const courseId = deleteBtn.getAttribute('data-course-id');
                const courseName = deleteBtn.getAttribute('data-course-name');

                if (courseId) {
                    openDeleteCourseModal(courseId, courseName);
                }
                return; // Important: stop further processing
            }

            // Otherwise, handle student deletion (existing code)
            let userId = deleteBtn.getAttribute('data-user-id');
            if (!userId) {
                const row = deleteBtn.closest('tr');
                if (row) {
                    userId = row.getAttribute('data-user-id');
                }
            }

            if (userId) {
                const studentRow = document.querySelector(`tr[data-user-id="${userId}"]`);
                let userName = 'Unknown Student';
                let userEmail = '';

                if (studentRow) {
                    const nameCell = studentRow.querySelector('td:nth-child(2)');
                    const emailCell = studentRow.querySelector('td:nth-child(3)');
                    if (nameCell) userName = nameCell.textContent;
                    if (emailCell) userEmail = emailCell.textContent;
                }

                openDeleteModal(userId, userName, userEmail);
            }
            return;
        }

        // Handle edit buttons (keep your existing code)
        const editBtn = e.target.closest('.btn-edit');
        if (editBtn && editBtn.hasAttribute('data-course-id')) {
            e.preventDefault();
            e.stopPropagation();

            const courseId = editBtn.dataset.courseId;
            const courseName = editBtn.dataset.courseName;
            const instName = editBtn.dataset.instName;
            const instEmail = editBtn.dataset.instEmail;
            const duration = editBtn.dataset.duration;

            // Populate the modal
            document.getElementById('inp-course-id').value = courseId;
            document.getElementById('inp-course-name').value = courseName;
            document.getElementById('inp-course-inst').value = instName;
            document.getElementById('inp-course-email').value = instEmail;
            document.getElementById('inp-course-dur').value = duration;

            // Update modal title
            const modal = document.getElementById('modal-course');
            const titleEl = modal.querySelector(".modal-title");
            if (titleEl) {
                titleEl.innerText = "Edit Course";
            }

            const submitBtn = modal.querySelector(".btn-add");
            if (submitBtn) submitBtn.innerText = "Update Course";

            openModal('course');
            return;
        }

        // ... rest of your code for other edit buttons
    });
}
// ==========================================
// NAVIGATION MANAGEMENT
// ==========================================
// ==========================================
// NAVIGATION MANAGEMENT (FIXED)
// ==========================================
// ==========================================
// NAVIGATION MANAGEMENT (ROBUST FIX)
// ==========================================
function setupNavigation() {
    const navLinks = document.querySelectorAll('.nav-link');

    // 1. Get current path, lowercase it, and remove trailing slash for consistency
    //    e.g. "/Instructor/Courses/" becomes "/instructor/courses"
    const currentPath = window.location.pathname.toLowerCase().replace(/\/+$/, "");

    navLinks.forEach(link => {
        const rawHref = link.getAttribute('href');

        // Safety check: ensure href exists
        if (!rawHref) return;

        // 2. Clean the link's href same as currentPath
        const linkPath = rawHref.toLowerCase().replace(/\/+$/, "");

        // 3. Compare
        if (currentPath === linkPath) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    // Optional: If you suspect old sessionStorage logic is interfering from cached files, clear it:
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

    setTimeout(() => {
        modal.classList.remove('hidden');
        modal.classList.add('step-enter');

        const firstInput = modal.querySelector('input, select, textarea');
        if (firstInput) firstInput.focus();
    }, 50);

    if (!currentlyEditingRow) {
        const titleEl = modal.querySelector(".modal-title");
        if (titleEl) {
            titleEl.innerText = `Add New ${type.charAt(0).toUpperCase() + type.slice(1)}`;
        }

        const submitBtn = modal.querySelector("button[type='submit']");
        if (submitBtn && type !== 'student') {
            submitBtn.innerText = `Save ${type.charAt(0).toUpperCase() + type.slice(1)}`;
        }
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
// DELETE STUDENT MODAL - FIXED VERSION
// ==========================================
function openDeleteModal(userId, userName, userEmail) {
    // Remove existing delete modal if any
    const existingModal = document.getElementById('modal-delete-student');
    if (existingModal) {
        existingModal.remove();
    }

    // Create fresh delete modal
    createDeleteModal();

    // Set the student details
    document.getElementById('delete-stud-id').value = userId;
    document.getElementById('delete-student-name').textContent = userName || 'Unknown Student';
    document.getElementById('delete-student-email').textContent = userEmail || 'No email available';

    // Update avatar with initials
    const avatar = document.querySelector('.student-avatar');
    if (avatar) {
        avatar.textContent = getInitials(userName);
    }

    // Open the modal
    currentModal = 'modal-delete-student';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-delete-student');

    if (overlay && modal) {
        overlay.classList.remove('hidden');
        overlay.style.opacity = '0';
        overlay.style.transition = 'opacity 0.3s ease';

        setTimeout(() => {
            overlay.style.opacity = '1';
        }, 10);

        modal.classList.remove('hidden');
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.8) translateY(-20px)';
        modal.style.transition = 'all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275)';

        setTimeout(() => {
            modal.style.opacity = '1';
            modal.style.transform = 'scale(1) translateY(0)';
        }, 50);
    }
}

// Helper function to get initials for avatar
function getInitials(name) {
    if (!name || name === 'Unknown Student') return "?";
    return name
        .split(' ')
        .map(word => word[0])
        .join('')
        .toUpperCase()
        .substring(0, 2);
}

// Create the enhanced delete modal
function createDeleteModal() {
    const overlay = document.getElementById('modal-overlay');

    const deleteModal = document.createElement('div');
    deleteModal.id = 'modal-delete-student';
    deleteModal.className = 'modal-box hidden delete-modal enhanced-delete-modal';
    deleteModal.innerHTML = `
        <div class="modal-header">
            <div class="warning-icon">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                    <line x1="12" y1="9" x2="12" y2="13"></line>
                    <line x1="12" y1="17" x2="12.01" y2="17"></line>
                </svg>
            </div>
            <h2 class="modal-title">Delete Student</h2>
            <p class="modal-subtitle">This action cannot be undone</p>
        </div>
        
        <div class="modal-body">
            <div class="confirmation-message">
                <p>Are you sure you want to permanently delete:</p>
                <div class="student-info-card">
                    <div class="student-avatar">?</div>
                    <div class="student-details">
                        <h3 id="delete-student-name" class="student-name">Loading...</h3>
                        <p id="delete-student-email" class="student-email">Loading...</p>
                    </div>
                </div>
                <div class="warning-list">
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>All student data will be permanently removed</span>
                    </div>
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>Course enrollments and grades will be deleted</span>
                    </div>
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>This action cannot be reversed</span>
                    </div>
                </div>
            </div>
            <input type="hidden" id="delete-stud-id" />
        </div>
        
        <div class="modal-footer">
            <button type="button" class="btn-cancel" onclick="cancelDelete()">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
                <span>Cancel</span>
            </button>
            <button type="button" class="btn-delete-confirm" onclick="performDeleteStudent()">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6"></polyline>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                    <line x1="10" y1="11" x2="10" y2="17"></line>
                    <line x1="14" y1="11" x2="14" y2="17"></line>
                </svg>
                <span>Delete Student</span>
            </button>
        </div>
    `;

    overlay.appendChild(deleteModal);
}

// Cancel delete modal
function cancelDelete() {
    const modal = document.getElementById('modal-delete-student');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.8) translateY(-20px)';
        overlay.style.opacity = '0';

        setTimeout(() => {
            modal.remove(); // Remove modal completely
            overlay.classList.add('hidden');
            document.body.classList.remove('modal-open');
            currentModal = null;

            overlay.style.opacity = '';
            overlay.style.transition = '';
        }, 300);
    }
}
function openEditCourseModal(courseId, courseName, instName, instEmail, duration) {
    // Populate the EDIT course modal (not the ADD course modal)
    document.getElementById('edit-course-id').value = courseId;
    document.getElementById('edit-course-name').value = courseName;
    document.getElementById('edit-course-inst').value = instName;
    document.getElementById('edit-course-dur').value = duration;

    // You might need to set status based on your model
    // document.getElementById('edit-course-status').value = status;

    // Open the correct modal
    currentModal = 'modal-edit-course';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-edit-course');

    if (overlay && modal) {
        overlay.classList.remove('hidden');
        modal.classList.remove('hidden');
    }
}
// Perform delete operation - FIXED VERSION
// Perform delete operation - FIXED (Notification Removed)
function performDeleteStudent() {
    const userId = document.getElementById('delete-stud-id').value;
    const deleteBtn = document.querySelector('#modal-delete-student .btn-delete-confirm');
    const originalContent = deleteBtn.innerHTML;
    const studentName = document.getElementById('delete-student-name').textContent;

    if (!userId) {
        alert('Invalid student ID'); // Changed from showNotification to standard alert
        return;
    }

    // Add loading state
    deleteBtn.classList.add('loading');
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = `
        <div class="spinner"></div>
        <span>Deleting ${studentName}...</span>
    `;

    // AJAX delete request
    fetch(`/Instructor/DeleteStudent?userId=${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (response.ok) {
                return response.text();
            }
            throw new Error('Failed to delete student');
        })
        .then(text => {
            if (text === "Student deleted successfully") {
                // Remove the row from table
                const row = document.querySelector(`tr[data-user-id="${userId}"]`);
                if (row) {
                    row.style.transition = 'all 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
                    row.style.opacity = '0';
                    row.style.transform = 'translateX(-100px) scale(0.9)';

                    setTimeout(() => {
                        row.remove();

                        // Update student count
                        const studentCountEl = document.getElementById('stat-students');
                        if (studentCountEl) {
                            const currentCount = parseInt(studentCountEl.textContent);
                            if (!isNaN(currentCount) && currentCount > 0) {
                                studentCountEl.textContent = currentCount - 1;
                            }
                        }

                        // ❌ REMOVED: showNotification('Student deleted successfully', 'success');

                        // Close modal after a short delay
                        setTimeout(() => {
                            cancelDelete();
                        }, 500);
                    }, 500);
                } else {
                    // If row not found, just close modal and reload
                    cancelDelete();

                    // ❌ REMOVED: showNotification('Student deleted successfully', 'success');

                    // Optionally reload after a delay to ensure consistency
                    setTimeout(() => {
                        window.location.reload();
                    }, 500);
                }
            } else {
                throw new Error('Failed to delete student');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // ❌ REMOVED: showNotification(...);
            alert('Error deleting student. Please try again.'); // Replaced with standard alert

            // Reset button state
            deleteBtn.classList.remove('loading');
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalContent;
        });
}
// ==========================================
// OTHER FUNCTIONS
// ==========================================
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

function incrementStat(id) {
    const statEl = document.getElementById(id);
    if (statEl) {
        let currentVal = parseInt(statEl.innerText);
        statEl.innerText = currentVal + 1;
    }
}

function decrementStat(id) {
    const statEl = document.getElementById(id);
    if (statEl) {
        let currentVal = parseInt(statEl.innerText);
        statEl.innerText = currentVal > 0 ? currentVal - 1 : 0;
    }
}

function animateCounter(elementId, start, end) {
    const element = document.getElementById(elementId);
    if (!element) return;

    let current = start;
    const step = start > end ? -1 : 1;
    const duration = 500;
    const increment = Math.abs(end - start) / (duration / 16);

    const timer = setInterval(() => {
        current += increment * step;
        if ((step > 0 && current >= end) || (step < 0 && current <= end)) {
            clearInterval(timer);
            current = end;
        }
        element.textContent = Math.round(current);
    }, 16);
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

// Add CSS styles (keep your existing styles, they're fine)
const style = document.createElement('style');
style.textContent = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
        20%, 40%, 60%, 80% { transform: translateX(5px); }
    }
    
    /* Enhanced Delete Modal Styles */
    .enhanced-delete-modal {
        max-width: 480px !important;
        width: 90% !important;
        padding: 0 !important;
        border-radius: 16px !important;
        border: 1px solid #fecaca;
        box-shadow: 0 20px 40px rgba(239, 68, 68, 0.15), 
                    0 10px 30px rgba(0, 0, 0, 0.1),
                    inset 0 1px 0 rgba(255, 255, 255, 0.1);
        overflow: hidden;
    }
    
    .modal-header {
        background: linear-gradient(135deg, #fee2e2 0%, #fecaca 100%);
        padding: 30px;
        text-align: center;
        border-bottom: 1px solid #fca5a5;
    }
    
    .warning-icon {
        color: #ef4444;
        margin-bottom: 15px;
        animation: pulse 2s infinite;
    }
    
    .modal-title {
        color: #dc2626;
        font-size: 24px;
        font-weight: 700;
        margin-bottom: 5px;
    }
    
    .modal-subtitle {
        color: #7f1d1d;
        font-size: 14px;
        opacity: 0.8;
    }
    
    .modal-body {
        padding: 30px;
        background: white;
    }
    
    .confirmation-message {
        text-align: center;
    }
    
    .confirmation-message p {
        color: #4b5563;
        margin-bottom: 20px;
        font-size: 16px;
    }
    
    .student-info-card {
        background: #f8fafc;
        border: 2px solid #e2e8f0;
        border-radius: 12px;
        padding: 20px;
        display: flex;
        align-items: center;
        gap: 15px;
        margin-bottom: 25px;
        transition: all 0.3s ease;
    }
    
    .student-info-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }
    
    .student-avatar {
        width: 56px;
        height: 56px;
        background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
        color: white;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 700;
        font-size: 20px;
        flex-shrink: 0;
    }
    
    .student-details {
        flex: 1;
        text-align: left;
    }
    
    .student-name {
        color: #1f2937;
        font-size: 18px;
        font-weight: 600;
        margin: 0 0 4px 0;
    }
    
    .student-email {
        color: #6b7280;
        font-size: 14px;
        margin: 0;
    }
    
    .warning-list {
        background: #fef2f2;
        border: 1px solid #fecaca;
        border-radius: 10px;
        padding: 20px;
        margin-top: 20px;
    }
    
    .warning-item {
        display: flex;
        align-items: flex-start;
        gap: 10px;
        margin-bottom: 12px;
        color: #7f1d1d;
        font-size: 14px;
    }
    
    .warning-item:last-child {
        margin-bottom: 0;
    }
    
    .warning-item svg {
        color: #ef4444;
        flex-shrink: 0;
        margin-top: 2px;
    }
    
    .modal-footer {
        padding: 25px 30px;
        background: #fafafa;
        border-top: 1px solid #e5e7eb;
        display: flex;
        gap: 15px;
        justify-content: flex-end;
    }
    
    .btn-cancel {
        background: white;
        border: 2px solid #d1d5db;
        color: #374151;
        padding: 12px 24px;
        border-radius: 10px;
        font-weight: 600;
        font-size: 14px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 8px;
        transition: all 0.3s ease;
    }
    
    .btn-cancel:hover {
        background: #f3f4f6;
        border-color: #9ca3af;
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }
    
    .btn-delete-confirm {
        background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
        border: none;
        color: white;
        padding: 12px 24px;
        border-radius: 10px;
        font-weight: 600;
        font-size: 14px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 8px;
        transition: all 0.3s ease;
        position: relative;
        overflow: hidden;
    }
    
    .btn-delete-confirm:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 20px rgba(239, 68, 68, 0.4);
    }
    
    .btn-delete-confirm:active {
        transform: translateY(0);
    }
    
    .btn-delete-confirm.loading {
        opacity: 0.8;
        cursor: not-allowed;
    }
    
    /* Spinner for loading state */
    .spinner {
        width: 16px;
        height: 16px;
        border: 2px solid rgba(255, 255, 255, 0.3);
        border-radius: 50%;
        border-top-color: white;
        animation: spin 1s ease-in-out infinite;
    }
    
    /* Success animation */
    .success-animation {
        text-align: center;
        padding: 20px 0;
    }
    
    .checkmark {
        width: 80px;
        height: 80px;
        margin: 0 auto;
    }
    
    .checkmark__circle {
        stroke: #10b981;
        stroke-width: 2;
        animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
    }
    
    .checkmark__check {
        stroke: #10b981;
        stroke-width: 2;
        stroke-linecap: round;
        stroke-linejoin: round;
        stroke-dasharray: 48;
        stroke-dashoffset: 48;
        animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.8s forwards;
    }
    
    /* Animations */
    @keyframes pulse {
        0%, 100% {
            transform: scale(1);
            opacity: 1;
        }
        50% {
            transform: scale(1.05);
            opacity: 0.8;
        }
    }
    
    @keyframes spin {
        to { transform: rotate(360deg); }
    }
    
    @keyframes stroke {
        100% {
            stroke-dashoffset: 0;
        }
    }
    
    .warning-shake {
        animation: shake 0.5s cubic-bezier(.36,.07,.19,.97) both;
    }
    
    .error-shake {
        animation: shake-hard 0.5s cubic-bezier(.36,.07,.19,.97) both;
    }
    
    @keyframes shake-hard {
        10%, 90% {
            transform: translate3d(-2px, 0, 0);
        }
        20%, 80% {
            transform: translate3d(4px, 0, 0);
        }
        30%, 50%, 70% {
            transform: translate3d(-6px, 0, 0);
        }
        40%, 60% {
            transform: translate3d(6px, 0, 0);
        }
    }
    
    /* Ripple effect */
    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.7);
        transform: scale(0);
        animation: ripple-animation 0.6s linear;
        pointer-events: none;
    }
    
    button, .action-btn {
        position: relative;
        overflow: hidden;
    }
    
    @keyframes ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    /* Responsive */
    @media (max-width: 640px) {
        .enhanced-delete-modal {
            width: 95% !important;
            margin: 10px !important;
        }
        
        .modal-header {
            padding: 20px;
        }
        
        .modal-body {
            padding: 20px;
        }
        
        .modal-footer {
            padding: 20px;
            flex-direction: column;
        }
        
        .btn-cancel,
        .btn-delete-confirm {
            width: 100%;
            justify-content: center;
        }
    }
`;

document.head.appendChild(style);

// Open delete course modal
function openDeleteCourseModal(courseId, courseName) {
    // Remove existing delete modal if any
    const existingModal = document.getElementById('modal-delete-course');
    if (existingModal) {
        existingModal.remove();
    }

    // Create fresh delete modal
    createDeleteCourseModal();

    // Set the course details
    document.getElementById('delete-course-id').value = courseId;
    document.getElementById('delete-course-name').textContent = courseName || 'Unknown Course';

    // Update avatar with initials
    const avatar = document.querySelector('.course-avatar');
    if (avatar) {
        avatar.textContent = getInitials(courseName);
    }

    // Open the modal
    currentModal = 'modal-delete-course';
    document.body.classList.add('modal-open');

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById('modal-delete-course');

    if (overlay && modal) {
        overlay.classList.remove('hidden');
        overlay.style.opacity = '0';
        overlay.style.transition = 'opacity 0.3s ease';

        setTimeout(() => {
            overlay.style.opacity = '1';
        }, 10);

        modal.classList.remove('hidden');
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.8) translateY(-20px)';
        modal.style.transition = 'all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275)';

        setTimeout(() => {
            modal.style.opacity = '1';
            modal.style.transform = 'scale(1) translateY(0)';
        }, 50);
    }
}

// Create the delete course modal
function createDeleteCourseModal() {
    const overlay = document.getElementById('modal-overlay');

    const deleteModal = document.createElement('div');
    deleteModal.id = 'modal-delete-course';
    deleteModal.className = 'modal-box hidden delete-modal enhanced-delete-modal';
    deleteModal.innerHTML = `
        <div class="modal-header">
            <div class="warning-icon">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
                    <line x1="12" y1="9" x2="12" y2="13"></line>
                    <line x1="12" y1="17" x2="12.01" y2="17"></line>
                </svg>
            </div>
            <h2 class="modal-title">Delete Course</h2>
            <p class="modal-subtitle">This action cannot be undone</p>
        </div>
        
        <div class="modal-body">
            <div class="confirmation-message">
                <p>Are you sure you want to permanently delete:</p>
                <div class="student-info-card">
                    <div class="course-avatar">?</div>
                    <div class="student-details">
                        <h3 id="delete-course-name" class="student-name">Loading...</h3>
                    </div>
                </div>
                <div class="warning-list">
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>All course data will be permanently removed</span>
                    </div>
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>Student enrollments will be removed</span>
                    </div>
                    <div class="warning-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <line x1="12" y1="8" x2="12" y2="12"></line>
                            <line x1="12" y1="16" x2="12.01" y2="16"></line>
                        </svg>
                        <span>This action cannot be reversed</span>
                    </div>
                </div>
            </div>
            <input type="hidden" id="delete-course-id" />
        </div>
        
        <div class="modal-footer">
            <button type="button" class="btn-cancel" onclick="cancelDeleteCourse()">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
                <span>Cancel</span>
            </button>
            <button type="button" class="btn-delete-confirm" onclick="performDeleteCourse()">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6"></polyline>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                    <line x1="10" y1="11" x2="10" y2="17"></line>
                    <line x1="14" y1="11" x2="14" y2="17"></line>
                </svg>
                <span>Delete Course</span>
            </button>
        </div>
    `;

    overlay.appendChild(deleteModal);
}

// Cancel delete course modal
function cancelDeleteCourse() {
    const modal = document.getElementById('modal-delete-course');
    const overlay = document.getElementById('modal-overlay');

    if (modal) {
        modal.style.opacity = '0';
        modal.style.transform = 'scale(0.8) translateY(-20px)';
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

// Perform delete course operation
function performDeleteCourse() {
    const courseId = document.getElementById('delete-course-id').value;
    const deleteBtn = document.querySelector('#modal-delete-course .btn-delete-confirm');
    const originalContent = deleteBtn.innerHTML;
    const courseName = document.getElementById('delete-course-name').textContent;

    if (!courseId) {
        alert('Invalid course ID');
        return;
    }

    // Add loading state
    deleteBtn.classList.add('loading');
    deleteBtn.disabled = true;
    deleteBtn.innerHTML = `<div class="spinner"></div><span>Deleting...</span>`;

    fetch(`/Instructor/DeleteCourse?courseId=${courseId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (response.ok) return response.text();
            // If server sends 500, throw error to catch block
            return response.text().then(text => { throw new Error(text) });
        })
        .then(text => {
            if (text === "Course deleted successfully") {
                const rows = document.querySelectorAll('#table-courses tbody tr');
                let targetRow = null;

                rows.forEach(row => {
                    const firstCell = row.querySelector('td:first-child');
                    // FIX: Use .trim() to ignore whitespace/newlines
                    if (firstCell && firstCell.textContent.trim() == courseId) {
                        targetRow = row;
                    }
                });

                if (targetRow) {
                    targetRow.style.transition = 'all 0.5s ease';
                    targetRow.style.opacity = '0';
                    setTimeout(() => {
                        targetRow.remove();
                        cancelDeleteCourse();

                        // Update stats
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
            // Reset button
            deleteBtn.classList.remove('loading');
            deleteBtn.disabled = false;
            deleteBtn.innerHTML = originalContent;

            // Show specific error if available
            alert('Deletion Failed: It seems this course has related data (like Questions or Topics) that cannot be deleted automatically.');
        });
}