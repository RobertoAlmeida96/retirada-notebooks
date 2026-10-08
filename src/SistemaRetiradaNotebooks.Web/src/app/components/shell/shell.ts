import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <app-navbar />
    <main class="page-content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .page-content { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
  `]
})
export class ShellComponent {}
