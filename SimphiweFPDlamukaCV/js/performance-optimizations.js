// Performance Optimizations for Modern Portfolio

class PerformanceOptimizer {
  constructor() {
    this.init();
  }

  init() {
    this.setupLazyLoading();
    this.setupImageOptimization();
    this.setupCriticalResourceHints();
    this.setupIntersectionObserverPolyfill();
    this.setupWebVitalsTracking();
  }

  // Lazy loading for images and content
  setupLazyLoading() {
    // Intersection Observer for lazy loading
    const lazyImages = document.querySelectorAll('img[data-src]');
    const lazyContent = document.querySelectorAll('.lazy-content');

    const imageObserver = new IntersectionObserver((entries, observer) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          const img = entry.target;
          
          // Create a new image to preload
          const newImg = new Image();
          newImg.onload = () => {
            img.src = img.dataset.src;
            img.classList.remove('lazy');
            img.classList.add('loaded');
          };
          newImg.src = img.dataset.src;
          
          observer.unobserve(img);
        }
      });
    }, {
      rootMargin: '50px 0px',
      threshold: 0.01
    });

    lazyImages.forEach(img => {
      img.classList.add('lazy');
      imageObserver.observe(img);
    });

    // Lazy load content sections
    const contentObserver = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('loaded');
        }
      });
    }, { threshold: 0.1 });

    lazyContent.forEach(content => contentObserver.observe(content));
  }

  // Image optimization and WebP support
  setupImageOptimization() {
    const supportsWebP = this.checkWebPSupport();
    
    if (supportsWebP) {
      document.querySelectorAll('img[data-webp]').forEach(img => {
        img.src = img.dataset.webp;
      });
    }
  }

  checkWebPSupport() {
    return new Promise(resolve => {
      const webP = new Image();
      webP.onload = webP.onerror = () => {
        resolve(webP.height === 2);
      };
      webP.src = 'data:image/webp;base64,UklGRjoAAABXRUJQVlA4IC4AAACyAgCdASoCAAIALmk0mk0iIiIiIgBoSygABc6WWgAA/veff/0PP8bA//LwYAAA';
    });
  }

  // Resource hints for better loading
  setupCriticalResourceHints() {
    const head = document.head;
    
    // Preconnect to external domains
    const preconnectDomains = [
      'https://fonts.googleapis.com',
      'https://fonts.gstatic.com'
    ];

    preconnectDomains.forEach(domain => {
      const link = document.createElement('link');
      link.rel = 'preconnect';
      link.href = domain;
      head.appendChild(link);
    });

    // Prefetch next page resources
    const prefetchResources = [
      '/css/modern-portfolio.css',
      '/js/modern-portfolio.js'
    ];

    prefetchResources.forEach(resource => {
      const link = document.createElement('link');
      link.rel = 'prefetch';
      link.href = resource;
      head.appendChild(link);
    });
  }

  // Polyfill for older browsers
  setupIntersectionObserverPolyfill() {
    if (!('IntersectionObserver' in window)) {
      // Fallback for older browsers
      const script = document.createElement('script');
      script.src = 'https://polyfill.io/v3/polyfill.min.js?features=IntersectionObserver';
      document.head.appendChild(script);
    }
  }

  // Web Vitals tracking
  setupWebVitalsTracking() {
    // Track Core Web Vitals
    this.trackLCP();
    this.trackFID();
    this.trackCLS();
  }

  trackLCP() {
    new PerformanceObserver((entryList) => {
      const entries = entryList.getEntries();
      const lastEntry = entries[entries.length - 1];
      console.log('LCP:', lastEntry.startTime);
    }).observe({ entryTypes: ['largest-contentful-paint'] });
  }

  trackFID() {
    new PerformanceObserver((entryList) => {
      const entries = entryList.getEntries();
      entries.forEach(entry => {
        console.log('FID:', entry.processingStart - entry.startTime);
      });
    }).observe({ entryTypes: ['first-input'] });
  }

  trackCLS() {
    let clsValue = 0;
    new PerformanceObserver((entryList) => {
      for (const entry of entryList.getEntries()) {
        if (!entry.hadRecentInput) {
          clsValue += entry.value;
        }
      }
      console.log('CLS:', clsValue);
    }).observe({ entryTypes: ['layout-shift'] });
  }
}

// Memory management
class MemoryManager {
  constructor() {
    this.observers = [];
    this.eventListeners = [];
    this.setupCleanup();
  }

  addObserver(observer) {
    this.observers.push(observer);
  }

  addEventListener(element, event, handler) {
    element.addEventListener(event, handler);
    this.eventListeners.push({ element, event, handler });
  }

  setupCleanup() {
    window.addEventListener('beforeunload', () => {
      this.cleanup();
    });
  }

  cleanup() {
    // Disconnect observers
    this.observers.forEach(observer => {
      if (observer.disconnect) {
        observer.disconnect();
      }
    });

    // Remove event listeners
    this.eventListeners.forEach(({ element, event, handler }) => {
      element.removeEventListener(event, handler);
    });

    // Clear arrays
    this.observers = [];
    this.eventListeners = [];
  }
}

// Debounced resize handler
class ResizeHandler {
  constructor() {
    this.callbacks = [];
    this.setupHandler();
  }

  addCallback(callback) {
    this.callbacks.push(callback);
  }

  setupHandler() {
    let resizeTimer;
    window.addEventListener('resize', () => {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(() => {
        this.callbacks.forEach(callback => callback());
      }, 250);
    });
  }
}

// Initialize performance optimizations
document.addEventListener('DOMContentLoaded', () => {
  new PerformanceOptimizer();
  
  // Global instances
  window.memoryManager = new MemoryManager();
  window.resizeHandler = new ResizeHandler();
});

// Service Worker registration
if ('serviceWorker' in navigator) {
  window.addEventListener('load', async () => {
    try {
      const registration = await navigator.serviceWorker.register('/sw.js');
      console.log('ServiceWorker registered successfully');
    } catch (error) {
      console.log('ServiceWorker registration failed');
    }
  });
}

// Critical CSS inlining for above-the-fold content
function inlineCriticalCSS() {
  const criticalCSS = `
    .hero { min-height: 100vh; display: flex; align-items: center; justify-content: center; }
    .hero-content { text-align: center; max-width: 800px; }
    .site-header { position: fixed; top: 0; width: 100%; z-index: 1000; }
  `;
  
  const style = document.createElement('style');
  style.textContent = criticalCSS;
  document.head.insertBefore(style, document.head.firstChild);
}

// Run critical CSS inlining immediately
inlineCriticalCSS();