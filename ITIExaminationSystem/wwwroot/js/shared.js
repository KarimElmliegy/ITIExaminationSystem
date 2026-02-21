// ===================================================================
// SHARED JAVASCRIPT - ITI EXAMINATION SYSTEM
// Common functions used across multiple pages
// ===================================================================

// ===================================================================
// GLOBAL STATE
// ===================================================================
let currentModal = null;

// ===================================================================
// MODAL MANAGEMENT
// ===================================================================

/**
 * Opens a modal by type
 * @param {string} type - The modal type (e.g., 'student', 'course', 'instructor')
 */
function openModal(type) {
    const modalId = `modal-${type}`;
    currentModal = modalId;

    const overlay = document.getElementById('modal-overlay');
    const modal = document.getElementById(modalId);

    if (!overlay || !modal) {
        console.warn(`Modal ${modalId} or overlay not found`);
        return;
    }

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
}

/**
 * Closes the currently open modal
 */
function closeModal() {
    if (!currentModal && !document.getElementById('modal-overlay')) return;

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

    // Reset multi-step forms
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');
    if (step1 && step2) {
        step1.classList.remove('hidden');
        step2.classList.add('hidden');
    }
}

// ===================================================================
// MULTI-STEP FORM NAVIGATION
// ===================================================================

/**
 * Shows step 2 of a multi-step form (validates step 1 first)
 */
function showStep2() {
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');

    if (!step1 || !step2) return;

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

/**
 * Shows step 1 of a multi-step form (goes back from step 2)
 */
function showStep1() {
    const step1 = document.getElementById('step-1');
    const step2 = document.getElementById('step-2');

    if (!step1 || !step2) return;

    step2.classList.add('step-exit');

    setTimeout(() => {
        step2.classList.add('hidden');
        step1.classList.remove('hidden');
        step1.classList.add('step-enter');
    }, 400);
}

// ===================================================================
// NOTIFICATION SYSTEM
// ===================================================================

/**
 * Shows a notification message
 * @param {string} message - The message to display
 * @param {string} type - The notification type ('success', 'error', 'info')
 */
function showNotification(message, type = 'info') {
    let container = document.querySelector('.notification-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'notification-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 10000;
            display: flex;
            flex-direction: column;
            gap: 10px;
        `;
        document.body.appendChild(container);
    }

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.style.cssText = `
        background: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6'};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        min-width: 300px;
        animation: slideInRight 0.3s ease;
        transition: all 0.3s ease;
    `;

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

// ===================================================================
// RIPPLE EFFECT UTILITY
// ===================================================================

/**
 * Creates a ripple effect on button click
 * @param {HTMLElement} button - The button element
 * @param {Event} event - The click event
 */
function createRippleEffect(button, event) {
    const existingRipples = button.querySelectorAll('.ripple');
    existingRipples.forEach(ripple => ripple.remove());

    const ripple = document.createElement('span');
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        left: ${x}px;
        top: ${y}px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.5);
        transform: scale(0);
        animation: ripple 0.6s ease-out;
        pointer-events: none;
    `;
    ripple.classList.add('ripple');

    button.style.position = 'relative';
    button.style.overflow = 'hidden';
    button.appendChild(ripple);

    setTimeout(() => {
        if (ripple.parentNode === button) {
            ripple.remove();
        }
    }, 600);
}

// Add ripple animation to document
if (!document.getElementById('ripple-animation-style')) {
    const style = document.createElement('style');
    style.id = 'ripple-animation-style';
    style.textContent = `
        @keyframes ripple {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }
    `;
    document.head.appendChild(style);
}

// ===================================================================
// FETCH API WRAPPER
// ===================================================================

/**
 * Makes a POST request with JSON data
 * @param {string} url - The endpoint URL
 * @param {object} data - The data to send
 * @returns {Promise} - The fetch promise
 */
async function postJSON(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || 'Request failed');
        }

        return await response.text();
    } catch (error) {
        console.error('POST request failed:', error);
        throw error;
    }
}

// ===================================================================
// HTML DECODE UTILITY
// ===================================================================

/**
 * Decodes HTML entities
 * @param {string} html - The HTML string to decode
 * @returns {string} - The decoded string
 */
function decodeHTML(html) {
    const txt = document.createElement('textarea');
    txt.innerHTML = html;
    return txt.value;
}

// ===================================================================
// INITIALIZATION
// ===================================================================

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Lucide icons
    if (window.lucide) {
        lucide.createIcons();
    }

    // Setup modal overlay click to close
    const overlay = document.getElementById('modal-overlay');
    if (overlay) {
        overlay.addEventListener('click', function(e) {
            if (e.target === this) {
                closeModal();
            }
        });
    }

    // Close modal on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && currentModal) {
            closeModal();
        }
    });

    // Add ripple effects to buttons
    document.querySelectorAll('button:not(.btn-delete), .action-btn:not(.btn-delete)').forEach(button => {
        button.addEventListener('click', function(e) {
            createRippleEffect(this, e);
        });
    });
});

// ===================================================================
// EXPORT FOR MODULE USAGE (if needed)
// ===================================================================
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        openModal,
        closeModal,
        showStep1,
        showStep2,
        showNotification,
        createRippleEffect,
        postJSON,
        decodeHTML
    };
}
