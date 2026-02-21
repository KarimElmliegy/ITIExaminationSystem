// ===================================================================
// ADMIN DASHBOARD JAVASCRIPT - ADMIN-SPECIFIC FUNCTIONALITY
// Common modal functions moved to shared.js
// ===================================================================

// Note: openModal, closeModal, showStep1, showStep2 are now in shared.js

const ITEMS_PER_PAGE = 6;
const carouselState = {
    students: { currentPage: 1, rows: [] },
    instructors: { currentPage: 1, rows: [] },
    courses: { currentPage: 1, rows: [] },
    branches: { currentPage: 1, rows: [] },
    tracks: { currentPage: 1, rows: [] }
};

// ================= TAB SWITCHING =================
function switchTab(tabName, button) {
    document.querySelectorAll('.content-tab').forEach(tab => {
        tab.classList.add('hidden');
    });

    document.querySelectorAll('.nav-link').forEach(btn => {
        btn.classList.remove('active');
    });

    document.getElementById(`tab-${tabName}`).classList.remove('hidden');

    if (button) {
        button.classList.add('active');
    }

    // Initialize carousel for the active tab
    setTimeout(() => initializeCarousel(tabName), 100);
}

// ================= UNIVERSAL CAROUSEL SYSTEM =================
function initializeCarousel(tabName) {
    const tableBody = document.getElementById(`${tabName}TableBody`);
    if (!tableBody) return;

    const rows = Array.from(tableBody.querySelectorAll('tr.carousel-row'));
    carouselState[tabName].rows = rows;

    if (rows.length === 0) {
        console.warn(`No data found for ${tabName}`);
        return;
    }

    renderCarouselPage(tabName, 1);
}

function renderCarouselPage(tabName, pageNumber) {
    const state = carouselState[tabName];
    state.currentPage = pageNumber;
    const rows = state.rows;
    const totalItems = rows.length;
    const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);

    // Update page indicators
    document.getElementById(`${tabName}CurrentPage`).textContent = pageNumber;
    document.getElementById(`${tabName}TotalPages`).textContent = totalPages;

    // Calculate start and end index
    const startIndex = (pageNumber - 1) * ITEMS_PER_PAGE;
    const endIndex = Math.min(startIndex + ITEMS_PER_PAGE, totalItems);

    // Show/hide rows
    rows.forEach((row, index) => {
        row.style.display = (index >= startIndex && index < endIndex) ? '' : 'none';
    });

    // Update button states
    document.getElementById(`${tabName}PrevBtn`).disabled = pageNumber === 1;
    document.getElementById(`${tabName}NextBtn`).disabled = pageNumber === totalPages;

    // Update dots
    renderCarouselDots(tabName, totalPages, pageNumber);
}

function moveCarousel(tabName, direction) {
    const state = carouselState[tabName];
    const totalItems = state.rows.length;
    const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
    const newPage = state.currentPage + direction;

    if (newPage >= 1 && newPage <= totalPages) {
        renderCarouselPage(tabName, newPage);
    }
}

function goToPage(tabName, pageNumber) {
    const state = carouselState[tabName];
    const totalItems = state.rows.length;
    const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);

    if (pageNumber >= 1 && pageNumber <= totalPages) {
        renderCarouselPage(tabName, pageNumber);
    }
}

function renderCarouselDots(tabName, totalPages, currentPage) {
    const dotsContainer = document.getElementById(`${tabName}CarouselDots`);
    if (!dotsContainer) return;

    dotsContainer.innerHTML = '';

    if (totalPages <= 1) return;

    for (let i = 1; i <= totalPages; i++) {
        const dot = document.createElement('button');
        dot.className = 'carousel-dot' + (i === currentPage ? ' active' : '');
        dot.type = 'button';
        dot.onclick = () => goToPage(tabName, i);
        dot.setAttribute('aria-label', `Go to page ${i}`);
        dotsContainer.appendChild(dot);
    }
}

// ================= EDIT MODAL FUNCTIONS =================
function openEditStudentModal(studentId, name, email, branch, track, intake) {
    document.getElementById('edit-student-id').value = studentId;
    document.getElementById('edit-stud-name').value = name;
    document.getElementById('edit-stud-email').value = email;
    document.getElementById('edit-stud-branch').value = branch;
    document.getElementById('edit-stud-track').value = track;
    document.getElementById('edit-stud-intake').value = intake;
    openModal('edit-student');
}

function openEditInstructorModal(instructorId, name, email) {
    document.getElementById('edit-instructor-id').value = instructorId;
    document.getElementById('edit-inst-name').value = name;
    document.getElementById('edit-inst-email').value = email;
    openModal('edit-instructor');
}

function openEditCourseModal(courseId, name, triesLimit) {
    document.getElementById('edit-course-id').value = courseId;
    document.getElementById('edit-course-name').value = name;
    document.getElementById('edit-course-tries').value = triesLimit || '';
    openModal('edit-course');
}

function openEditBranchModal(branchId, name, location) {
    document.getElementById('edit-branch-id').value = branchId;
    document.getElementById('edit-branch-name').value = name;
    document.getElementById('edit-branch-location').value = location;
    openModal('edit-branch');
}

function openEditTrackModal(trackId, name) {
    document.getElementById('edit-track-id').value = trackId;
    document.getElementById('edit-track-name').value = name;
    openModal('edit-track');
}

// ================= DELETE MODAL FUNCTION =================
function openDeleteModal(id, entityType, name = '', email = '', additionalInfo = '') {
    document.getElementById('delete-id').value = id;
    document.getElementById('delete-entity-type').value = entityType;

    // Set modal title based on entity type
    const modalTitle = document.getElementById('delete-title');
    modalTitle.textContent = `Delete ${entityType.charAt(0).toUpperCase() + entityType.slice(1)}`;

    // Set the target name
    document.getElementById('delete-target-name').textContent = name || 'this item';

    // Set warning details based on entity type
    const warningDetails = document.getElementById('delete-warning-details');
    if (entityType === 'student') {
        warningDetails.textContent = 'This action cannot be undone. This will permanently delete the student account and remove all associated data from the system.';
    } else if (entityType === 'instructor') {
        warningDetails.textContent = 'This action cannot be undone. This will permanently delete the instructor account and remove all associated data from the system.';
    } else if (entityType === 'course') {
        warningDetails.textContent = 'This action cannot be undone. This will permanently delete the course and remove all associated exams and questions from the system.';
    } else if (entityType === 'branch') {
        warningDetails.textContent = 'This action cannot be undone. This will permanently delete the branch and reassign all associated students to other branches.';
    } else if (entityType === 'track') {
        warningDetails.textContent = 'This action cannot be undone. This will permanently delete the track and reassign all associated students to other tracks.';
    }

    openModal('delete-confirm');
}

// ================= API CALLS =================
async function handleAddStudent() {
    const fullName = document.getElementById('inp-stud-name').value;
    const email = document.getElementById('inp-stud-email').value;
    const password = document.getElementById('inp-stud-password').value;
    const confirmPassword = document.getElementById('inp-stud-confirm').value;
    const branchId = document.getElementById('inp-stud-branch').value;
    const trackId = document.getElementById('inp-stud-track').value;
    const intakeNumber = document.getElementById('inp-stud-intake').value;

    if (password !== confirmPassword) {
        alert('Passwords do not match');
        return;
    }

    const data = {
        FullName: fullName,
        Email: email,
        Password: password,
        BranchId: parseInt(branchId),
        TrackId: parseInt(trackId),
        IntakeNumber: parseInt(intakeNumber)
    };

    try {
        const response = await fetch('/Admin/AddStudent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Student added successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error adding student');
    }
}

async function handleUpdateStudent() {
    const studentId = document.getElementById('edit-student-id').value;
    const fullName = document.getElementById('edit-stud-name').value;
    const email = document.getElementById('edit-stud-email').value;
    const branchId = document.getElementById('edit-stud-branch').value;
    const trackId = document.getElementById('edit-stud-track').value;
    const intakeNumber = document.getElementById('edit-stud-intake').value;

    const data = {
        StudentId: parseInt(studentId),
        FullName: fullName,
        Email: email,
        BranchId: parseInt(branchId),
        TrackId: parseInt(trackId),
        IntakeNumber: parseInt(intakeNumber)
    };

    try {
        const response = await fetch('/Admin/UpdateStudent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Student updated successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error updating student');
    }
}

async function handleAddInstructor() {
    const fullName = document.getElementById('inp-inst-name').value;
    const email = document.getElementById('inp-inst-email').value;
    const password = document.getElementById('inp-inst-password').value;
    const confirmPassword = document.getElementById('inp-inst-confirm').value;

    if (password !== confirmPassword) {
        alert('Passwords do not match');
        return;
    }

    const data = {
        FullName: fullName,
        Email: email,
        Password: password,
        ConfirmPassword: confirmPassword
    };

    try {
        const response = await fetch('/Admin/AddInstructor', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Instructor added successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error adding instructor');
    }
}

async function handleUpdateInstructor() {
    const instructorId = document.getElementById('edit-instructor-id').value;
    const fullName = document.getElementById('edit-inst-name').value;
    const email = document.getElementById('edit-inst-email').value;
    const password = document.getElementById('edit-inst-password').value;

    const data = {
        InstructorId: parseInt(instructorId),
        FullName: fullName,
        Email: email,
        Password: password || null
    };

    try {
        const response = await fetch('/Admin/UpdateInstructor', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Instructor updated successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error updating instructor');
    }
}

async function handleAddCourse() {
    const courseName = document.getElementById('inp-course-name').value;
    const triesLimit = document.getElementById('inp-course-tries').value;

    const data = {
        CourseName: courseName,
        TriesLimit: triesLimit ? parseInt(triesLimit) : null,
        Duration: 40 // Default duration
    };

    try {
        const response = await fetch('/Admin/AddCourse', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Course added successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error adding course');
    }
}

async function handleUpdateCourse() {
    const courseId = document.getElementById('edit-course-id').value;
    const courseName = document.getElementById('edit-course-name').value;
    const triesLimit = document.getElementById('edit-course-tries').value;

    const data = {
        CourseId: parseInt(courseId),
        CourseName: courseName,
        TriesLimit: triesLimit ? parseInt(triesLimit) : null,
        Duration: 40 // Default duration
    };

    try {
        const response = await fetch('/Admin/UpdateCourse', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Course updated successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error updating course');
    }
}

async function handleAddBranch() {
    const branchName = document.getElementById('inp-branch-name').value;
    const branchLocation = document.getElementById('inp-branch-location').value;

    const data = {
        BranchName: branchName,
        BranchLocation: branchLocation
    };

    try {
        const response = await fetch('/Admin/AddBranch', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Branch added successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error adding branch');
    }
}

async function handleUpdateBranch() {
    const branchId = document.getElementById('edit-branch-id').value;
    const branchName = document.getElementById('edit-branch-name').value;
    const branchLocation = document.getElementById('edit-branch-location').value;

    const data = {
        BranchId: parseInt(branchId),
        BranchName: branchName,
        BranchLocation: branchLocation
    };

    try {
        const response = await fetch('/Admin/UpdateBranch', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Branch updated successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error updating branch');
    }
}

async function handleAddTrack() {
    const trackName = document.getElementById('inp-track-name').value;

    const data = { TrackName: trackName };

    try {
        const response = await fetch('/Admin/AddTrack', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Track added successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error adding track');
    }
}

async function handleUpdateTrack() {
    const trackId = document.getElementById('edit-track-id').value;
    const trackName = document.getElementById('edit-track-name').value;

    const data = {
        TrackId: parseInt(trackId),
        TrackName: trackName
    };

    try {
        const response = await fetch('/Admin/UpdateTrack', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            alert('Track updated successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error updating track');
    }
}

async function performDelete() {
    const id = document.getElementById('delete-id').value;
    const entityType = document.getElementById('delete-entity-type').value;

    if (!id || !entityType) {
        alert('Missing information');
        return;
    }

    let endpoint = '';
    let paramName = '';

    switch (entityType) {
        case 'student':
            endpoint = '/Admin/DeleteStudent';
            paramName = 'studentId';
            break;
        case 'instructor':
            endpoint = '/Admin/DeleteInstructor';
            paramName = 'instructorId';
            break;
        case 'course':
            endpoint = '/Admin/DeleteCourse';
            paramName = 'courseId';
            break;
        case 'branch':
            endpoint = '/Admin/DeleteBranch';
            paramName = 'branchId';
            break;
        case 'track':
            endpoint = '/Admin/DeleteTrack';
            paramName = 'trackId';
            break;
        default:
            alert('Invalid entity type');
            return;
    }

    const url = `${endpoint}?${paramName}=${id}`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (response.ok) {
            alert('Deleted successfully');
            closeModal();
            window.location.reload();
        } else {
            const error = await response.text();
            alert('Error: ' + error);
        }
    } catch (err) {
        console.error(err);
        alert('Error deleting item');
    }
}

// ================= INITIALIZATION =================
document.addEventListener('DOMContentLoaded', function () {
    // Initialize Lucide icons
    if (window.lucide) {
        lucide.createIcons();
    }

    // Initialize carousels for all tabs
    ['students', 'instructors', 'courses', 'branches', 'tracks'].forEach(tab => {
        setTimeout(() => initializeCarousel(tab), 100);
    });

    // Handle delete button clicks
    document.addEventListener('click', function (e) {
        const deleteBtn = e.target.closest('.btn-delete');
        if (deleteBtn) {
            e.preventDefault();

            const entityType = deleteBtn.getAttribute('data-entity-type');
            let id = '';
            let name = '';

            switch (entityType) {
                case 'student':
                    id = deleteBtn.getAttribute('data-student-id');
                    name = deleteBtn.getAttribute('data-user-name');
                    break;
                case 'instructor':
                    id = deleteBtn.getAttribute('data-instructor-id');
                    name = deleteBtn.getAttribute('data-user-name');
                    break;
                case 'course':
                    id = deleteBtn.getAttribute('data-course-id');
                    name = deleteBtn.getAttribute('data-course-name');
                    break;
                case 'branch':
                    id = deleteBtn.getAttribute('data-branch-id');
                    name = deleteBtn.getAttribute('data-branch-name');
                    break;
                case 'track':
                    id = deleteBtn.getAttribute('data-track-id');
                    name = deleteBtn.getAttribute('data-track-name');
                    break;
            }

            if (id) {
                openDeleteModal(id, entityType, name);
            }
        }
    });

    // Show students tab by default
    switchTab('students', document.getElementById('btn-students'));

    // Keyboard navigation for carousel
    document.addEventListener('keydown', function (e) {
        if (e.target.tagName !== 'INPUT' &&
            e.target.tagName !== 'TEXTAREA' &&
            e.target.tagName !== 'SELECT') {

            const activeTab = document.querySelector('.content-tab:not(.hidden)').id.replace('tab-', '');

            if (e.key === 'ArrowLeft') {
                moveCarousel(activeTab, -1);
            } else if (e.key === 'ArrowRight') {
                moveCarousel(activeTab, 1);
            }
        }
    });
});