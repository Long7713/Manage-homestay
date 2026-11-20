
    document.addEventListener('DOMContentLoaded', () => {
      const track = document.getElementById('roomsCarouselTrack');
      const btnLeft = document.getElementById('scrollLeftBtn');
      const btnRight = document.getElementById('scrollRightBtn');

      if (!track) return;

      // Scroll with navigation buttons
      const scrollStep = 320;

      if (btnLeft) {
        btnLeft.addEventListener('click', () => {
          track.scrollBy({ left: -scrollStep, behavior: 'smooth' });
        });
      }

      if (btnRight) {
        btnRight.addEventListener('click', () => {
          track.scrollBy({ left: scrollStep, behavior: 'smooth' });
        });
      }

      // Drag to scroll functionality
      let isDown = false;
      let startX;
      let scrollLeft;

      track.addEventListener('mousedown', (e) => {
        isDown = true;
        track.classList.add('active');
        startX = e.pageX - track.offsetLeft;
        scrollLeft = track.scrollLeft;
      });

      track.addEventListener('mouseleave', () => {
        isDown = false;
        track.classList.remove('active');
      });

      track.addEventListener('mouseup', () => {
        isDown = false;
        track.classList.remove('active');
      });

      track.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - track.offsetLeft;
        const walk = (x - startX) * 1.6; // Scroll speed multiplier
        track.scrollLeft = scrollLeft - walk;
      });
    });
  