// ==========================================
// DASHBOARD SPECIFIC JAVASCRIPT
// ==========================================
let currentlyEditingRow = null;
const modalOverlay = document.getElementById("modal-overlay");
const logoutBtn = document.getElementById("logoutBtn");

// ==========================================
// INITIALIZATION (Run when page loads)
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
// LOGOUT LOGIC
// ==========================================
if (logoutBtn) {
    logoutBtn.addEventListener("click", (e) => {
        e.preventDefault();
        window.location.href = "/Home/LoginPage";
    });
}

// Navigation active state
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
// EDIT STUDENT MODAL
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
// EDIT INSTRUCTOR MODAL
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
// EDIT COURSE MODAL
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
// STATS UPDATER
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
// TABLE ACTIONS SETUP
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
// STUDENT WIZARD LOGIC
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
// INTAKE ROLLER SETUP
// ==========================================
function setupIntakeRoller() {
    const valueSpan = document.getElementById("rollerValue");
    const upBtn = document.querySelector(".roller-btn[data-step='1']");
    const downBtn = document.querySelector(".roller-btn[data-step='-1']");

    if (!valueSpan || !upBtn || !downBtn) return;

    const min = 1;
    const max = 46;
    let currentIntake = parseInt(valueSpan.textContent);

    // UP button
    upBtn.addEventListener("click", function (e) {
        e.preventDefault();
        let newIntake = currentIntake + 1;
        if (newIntake <= max) {
            window.location.href = `/Instructor/FilterByIntake?intake=${newIntake}`;
        }
    });

    // DOWN button
    downBtn.addEventListener("click", function (e) {
        e.preventDefault();
        let newIntake = currentIntake - 1;
        if (newIntake >= min) {
            window.location.href = `/Instructor/FilterByIntake?intake=${newIntake}`;
        }
    });
}

function showStep1() {
    document.getElementById('step-2').classList.add('hidden');
    document.getElementById('step-1').classList.remove('hidden');
}

// ==========================================
// FORM HANDLERS
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
// CLOSE MODAL ON OUTSIDE CLICK
// ==========================================
if (modalOverlay) {
    modalOverlay.addEventListener('click', function (e) {
        if (e.target === this) {
            closeModal();
        }
    });
}

// ==========================================
// CLOSE MODAL WITH ESCAPE KEY
// ==========================================
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeModal();
    }
});

// ==========================================
// TAB SWITCHING FUNCTION (if needed)
// ==========================================
function switchTab(tabName, element) {
    // Hide all tab contents
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.add('hidden');
    });

    // Remove active class from all nav links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });

    // Show selected tab content
    const tabContent = document.getElementById(`tab-${tabName}`);
    if (tabContent) {
        tabContent.classList.remove('hidden');
    }

    // Add active class to clicked nav link
    if (element) {
        element.classList.add('active');
    }
}