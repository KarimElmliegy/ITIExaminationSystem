// ==========================================
// 1. GLOBAL STATE & DOM ELEMENTS
// ==========================================
let currentlyEditingRow = null;

const loginView = document.getElementById("login-view");
const dashboardView = document.getElementById("dashboard-view");
const loginForm = document.getElementById("loginForm");
const logoutBtn = document.getElementById("logoutBtn");
const modalOverlay = document.getElementById("modal-overlay");

// ==========================================
// 2. INITIALIZATION (Run when page loads)
// ==========================================
document.addEventListener("DOMContentLoaded", function () {
    // 1. Initialize Icons
    if (window.lucide) {
        lucide.createIcons();
    }

    // 2. DEFAULT TAB LOGIC
    const btnStudents = document.querySelector('.nav-link.active');
    if (btnStudents) {
        switchTab('students', btnStudents);
    }

    // 3. Setup Table Actions (Edit/Delete)
    setupTableActions("table-students", "student", ["inp-stud-name", "inp-stud-email", "inp-stud-track"]);
    setupTableActions("table-instructors", "instructor", ["inp-inst-name", "inp-inst-email", "inp-inst-spec"]);
    setupTableActions("table-courses", "course", ["inp-course-name", "inp-course-inst", "inp-course-dur"]);

    // 4. Intake Roller Setup
    setupIntakeRoller();
});

// ==========================================
// 3. LOGOUT LOGIC
// ==========================================
if (logoutBtn) {
    logoutBtn.addEventListener("click", (e) => {
        e.preventDefault();
        window.location.href = "/Home/LoginPage";
    });
}
document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('.nav-link');

    // Set active link based on current page URL
    const currentPath = window.location.pathname;

    navLinks.forEach(link => {
        // Check if link's href matches current page
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }

        // Optional: Store clicked link in sessionStorage
        link.addEventListener('click', function (e) {
            // Store the href of clicked link
            sessionStorage.setItem('activeNav', this.getAttribute('href'));
        });
    });

    // Alternative: Use sessionStorage to persist active state
    const storedActive = sessionStorage.getItem('activeNav');
    if (storedActive) {
        navLinks.forEach(link => {
            if (link.getAttribute('href') === storedActive) {
                link.classList.add('active');
            }
        });
    }
});

function openModal(type) {
    // Show the dark overlay
    if (modalOverlay) {
        modalOverlay.classList.remove("hidden");
    }

    // Hide all modal boxes
    document.querySelectorAll(".modal-box").forEach((box) => box.classList.add("hidden"));

    // Show the specific modal
    const targetModal = document.getElementById(`modal-${type}`);
    if (targetModal) {
        targetModal.classList.remove("hidden");

        // Prevent body scroll
        document.body.classList.add('modal-open');

        // Set title for adding new
        if (!currentlyEditingRow) {
            const titleEl = targetModal.querySelector(".modal-title");
            if (titleEl) {
                titleEl.innerText = `Add New ${type.charAt(0).toUpperCase() + type.slice(1)}`;
            }

            const submitBtn = targetModal.querySelector("button[type='submit']");
            if (submitBtn && type !== 'student') {
                submitBtn.innerText = `Save ${type.charAt(0).toUpperCase() + type.slice(1)}`;
            }
        }
    }
}

function closeModal() {
    // Re-enable body scroll
    document.body.classList.remove('modal-open');

    // Hide overlay
    if (modalOverlay) {
        modalOverlay.classList.add("hidden");
    }

    // Hide all modal boxes
    const modals = document.querySelectorAll('.modal-box');
    modals.forEach(modal => modal.classList.add('hidden'));

    // Clear all form inputs
    document.querySelectorAll("form").forEach((f) => f.reset());

    // Reset Steps (for Student Modal)
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');
    if (step1 && step2) {
        step1.classList.remove('hidden');
        step2.classList.add('hidden');
    }

    // Reset editing state
    currentlyEditingRow = null;
}

// ==========================================
// 6. EDIT STUDENT MODAL
// ==========================================
function openEditStudentModal(userId, name, email, branch, track, intake) {
    // Populate form fields
    document.getElementById('edit-stud-id').value = userId;
    document.getElementById('edit-stud-name').value = name;
    document.getElementById('edit-stud-email').value = email;
    document.getElementById('edit-stud-branch').value = branch;
    document.getElementById('edit-stud-track').value = track;
    document.getElementById('edit-stud-intake').value = intake;

    // Prevent body scroll
    document.body.classList.add('modal-open');

    // Show modal overlay
    if (modalOverlay) {
        modalOverlay.classList.remove('hidden');
    }

    // Hide all other modals first
    document.querySelectorAll('.modal-box').forEach(modal => modal.classList.add('hidden'));

    // Show edit student modal
    const editModal = document.getElementById('modal-edit-student');
    if (editModal) {
        editModal.classList.remove('hidden');
    }
}

// ==========================================
// 7. EDIT INSTRUCTOR MODAL
// ==========================================
function openEditInstructorModal(userId, name, email, specialization) {
    // Populate form fields
    document.getElementById('edit-inst-id').value = userId;
    document.getElementById('edit-inst-name').value = name;
    document.getElementById('edit-inst-email').value = email;
    document.getElementById('edit-inst-spec').value = specialization;

    // Prevent body scroll
    document.body.classList.add('modal-open');

    // Show modal
    if (modalOverlay) {
        modalOverlay.classList.remove('hidden');
    }

    // Hide all other modals
    document.querySelectorAll('.modal-box').forEach(modal => modal.classList.add('hidden'));

    // Show edit instructor modal
    const editModal = document.getElementById('modal-edit-instructor');
    if (editModal) {
        editModal.classList.remove('hidden');
    }
}

// ==========================================
// 8. EDIT COURSE MODAL
// ==========================================
function openEditCourseModal(courseId, courseName, instructor, duration, status) {
    // Populate form fields
    document.getElementById('edit-course-id').value = courseId;
    document.getElementById('edit-course-name').value = courseName;
    document.getElementById('edit-course-inst').value = instructor;
    document.getElementById('edit-course-dur').value = duration;
    document.getElementById('edit-course-status').value = status;

    // Prevent body scroll
    document.body.classList.add('modal-open');

    // Show modal
    if (modalOverlay) {
        modalOverlay.classList.remove('hidden');
    }

    // Hide all other modals
    document.querySelectorAll('.modal-box').forEach(modal => modal.classList.add('hidden'));

    // Show edit course modal
    const editModal = document.getElementById('modal-edit-course');
    if (editModal) {
        editModal.classList.remove('hidden');
    }
}

// ==========================================
// 9. STATS UPDATER
// ==========================================
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

// ==========================================
// 10. TABLE ACTIONS SETUP
// ==========================================
function setupTableActions(tableId, modalType, inputs) {
    const table = document.getElementById(tableId);
    const statId = "stat-" + tableId.split("-")[1];

    if (!table) return;

    table.addEventListener("click", (e) => {
        const target = e.target;
        const btnEdit = target.closest(".btn-edit");
        const btnDelete = target.closest(".btn-delete");
        const row = target.closest("tr");

        // DELETE LOGIC
        if (btnDelete) {
            e.preventDefault();
            if (confirm("Are you sure you want to delete this record?")) {
                row.remove();
                decrementStat(statId);
            }
            return;
        }

        // EDIT LOGIC (for non-link buttons)
        if (btnEdit) {
            if (btnEdit.tagName === 'A') return; // Let links navigate normally

            currentlyEditingRow = row;

            const data1 = row.cells[1].innerText;
            const data2 = row.cells[2].innerText;

            if (inputs.length > 0) document.getElementById(inputs[0]).value = data1;
            if (inputs.length > 1) document.getElementById(inputs[1]).value = data2;

            const modal = document.getElementById(`modal-${modalType}`);
            if (modal) {
                const titleEl = modal.querySelector(".modal-title");
                if (titleEl) {
                    titleEl.innerText = `Edit ${modalType.charAt(0).toUpperCase() + modalType.slice(1)}`;
                }

                const submitBtn = modal.querySelector(".btn-add");
                if (submitBtn) submitBtn.innerText = "Update";
            }

            openModal(modalType);
        }
    });
}

// ==========================================
// 11. STUDENT WIZARD LOGIC
// ==========================================
function showStep2() {
    const step1Div = document.getElementById('step-1');
    const inputs = step1Div.querySelectorAll('input');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.checkValidity()) {
            isValid = false;
            input.reportValidity();
        }
    });

    if (isValid) {
        document.getElementById('step-1').classList.add('hidden');
        document.getElementById('step-2').classList.remove('hidden');
    }
}

// ==========================================
// 13. INTAKE ROLLER SETUP
// ==========================================
// ==========================================
// 13. INTAKE ROLLER SETUP
// ==========================================

function showStep1() {
    document.getElementById('step-2').classList.add('hidden');
    document.getElementById('step-1').classList.remove('hidden');
}

// ==========================================
// 12. FORM HANDLERS
// ==========================================
function handleAddCourse(e) {
    e.preventDefault();
    // Your logic here
    closeModal();
}

function handleAddInstructor(e) {
    e.preventDefault();
    // Your logic here
    closeModal();
}

// ==========================================
// 13. INTAKE ROLLER SETUP
// ==========================================

// ==========================================
// 14. CLOSE MODAL ON OUTSIDE CLICK
// ==========================================
if (modalOverlay) {
    modalOverlay.addEventListener('click', function (e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

// ==========================================
// 15. CLOSE MODAL WITH ESCAPE KEY
// ==========================================
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeModal();
    }
});


//////////////////////////////
//////////////////////////////
//////////////////////////////
//////////////////////////////
//////////////////////////////

// Initialize Lucide icons
lucide.createIcons();

// Update time and date
function updateDateTime() {
    const now = new Date();

    // Format time
    const time = now.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    });

    // Format date
    const date = now.toLocaleDateString('en-US', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });

    document.getElementById('current-time').textContent = time;
    document.getElementById('current-date').textContent = date;
}

// Update every minute
updateDateTime();
setInterval(updateDateTime, 60000);

// Password toggle
const togglePassword = document.getElementById('togglePassword');
const passwordInput = document.getElementById('password');

if (togglePassword && passwordInput) {
    togglePassword.addEventListener('click', function () {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        const eyeIcon = this.querySelector('.eye-icon');
        if (eyeIcon) {
            eyeIcon.setAttribute('data-lucide', type === 'password' ? 'eye' : 'eye-off');
            lucide.createIcons();
        }
    });
}

// Floating label animation
document.querySelectorAll('.modern-input').forEach(input => {
    input.addEventListener('focus', function () {
        this.parentElement.classList.add('focused');
    });

    input.addEventListener('blur', function () {
        if (!this.value) {
            this.parentElement.classList.remove('focused');
        }
    });

    // Check initial value
    if (input.value) {
        input.parentElement.classList.add('focused');
    }
});

// Form submission with animation
const loginForm = document.getElementById('loginFormMVC');
const loginButton = document.getElementById('loginButton');

if (loginForm && loginButton) {
    loginForm.addEventListener('submit', function (e) {
        const buttonText = loginButton.querySelector('.button-text');
        const buttonIcon = loginButton.querySelector('.button-icon');
        const buttonLoader = loginButton.querySelector('.button-loader');

        if (buttonText && buttonIcon && buttonLoader) {
            // Show loading state
            buttonText.textContent = 'Authenticating...';
            buttonIcon.style.opacity = '0';
            buttonLoader.style.display = 'flex';
            loginButton.disabled = true;

            // Add pulse animation
            loginButton.classList.add('pulse');
        }
    });
}

// Animate stats on load
function animateStats() {
    const stats = ['stat-students', 'stat-courses', 'stat-exams'];
    const targets = [2847, 128, 5234];

    stats.forEach((statId, index) => {
        const element = document.getElementById(statId);
        if (!element) return;

        const target = targets[index];
        let current = 0;
        const increment = target / 50;
        const duration = 1500;
        const stepTime = duration / 50;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            element.textContent = Math.floor(current).toLocaleString();
        }, stepTime);
    });
}

// Start animations when page loads
document.addEventListener('DOMContentLoaded', function () {
    // Animate stats after a delay
    setTimeout(animateStats, 1000);

    // Add hover effects to cards
    document.querySelectorAll('.preview-card').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-8px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
});

// Add ripple effect to buttons
document.querySelectorAll('.login-button, .social-button').forEach(button => {
    button.addEventListener('click', function (e) {
        const x = e.clientX - e.target.getBoundingClientRect().left;
        const y = e.clientY - e.target.getBoundingClientRect().top;

        const ripple = document.createElement('span');
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';
        ripple.classList.add('ripple');

        this.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    });
});

