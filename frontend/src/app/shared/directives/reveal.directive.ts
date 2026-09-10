import { AfterViewInit, Directive, ElementRef, OnDestroy, inject, signal } from '@angular/core';

@Directive({
  selector: '[appReveal]',
  host: {
    class: 'app-reveal',
    '[class.is-revealed]': 'revealed()',
  },
})
export class RevealDirective implements AfterViewInit, OnDestroy {
  private readonly host = inject(ElementRef<HTMLElement>);
  private observer?: IntersectionObserver;
  private fallbackId?: ReturnType<typeof setTimeout>;
  readonly revealed = signal(false);

  ngAfterViewInit(): void {
    this.fallbackId = setTimeout(() => this.reveal(), 80);

    if (typeof IntersectionObserver === 'undefined') {
      this.reveal();
      return;
    }

    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          this.reveal();
        }
      },
      { threshold: 0, rootMargin: '40px' },
    );

    this.observer.observe(this.host.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
    if (this.fallbackId) {
      clearTimeout(this.fallbackId);
    }
  }

  private reveal(): void {
    this.revealed.set(true);
    this.observer?.disconnect();
  }
}
