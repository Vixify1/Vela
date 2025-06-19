// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Mobile Navigation Toggle Functionality
document.addEventListener('DOMContentLoaded', function() {
    const mobileMenuToggle = document.querySelector('.mobile-menu-toggle');
    const mobileCloseBtn = document.querySelector('.mobile-close-btn');
    const sidebar = document.querySelector('.sidebar');
    const mobileOverlay = document.querySelector('.mobile-overlay');
    
    // Function to open mobile menu
    function openMobileMenu() {
        if (sidebar && mobileOverlay) {
            sidebar.classList.add('mobile-active');
            mobileOverlay.classList.add('active');
            document.body.style.overflow = 'hidden'; // Prevent scrolling when menu is open
        }
    }
    
    // Function to close mobile menu
    function closeMobileMenu() {
        if (sidebar && mobileOverlay) {
            sidebar.classList.remove('mobile-active');
            mobileOverlay.classList.remove('active');
            document.body.style.overflow = ''; // Restore scrolling
        }
    }
    
    // Toggle mobile menu when hamburger button is clicked
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function(e) {
            e.preventDefault();
            openMobileMenu();
        });
    }
    
    // Close mobile menu when close button is clicked
    if (mobileCloseBtn) {
        mobileCloseBtn.addEventListener('click', function(e) {
            e.preventDefault();
            closeMobileMenu();
        });
    }
    
    // Close mobile menu when overlay is clicked
    if (mobileOverlay) {
        mobileOverlay.addEventListener('click', function() {
            closeMobileMenu();
        });
    }
    
    // Close mobile menu when a navigation link is clicked (optional - for better UX)
    const navLinks = document.querySelectorAll('.sidebar .nav-link');
    navLinks.forEach(function(link) {
        link.addEventListener('click', function() {
            // Only close on mobile
            if (window.innerWidth <= 768) {
                closeMobileMenu();
            }
        });
    });
    
    // Close mobile menu when window is resized to desktop size
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            closeMobileMenu();
        }
    });
});
