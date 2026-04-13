import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from "./shared/components/footer/footer";
import { Navbar } from "./shared/components/navbar/navbar";
import { TopNavbar } from "./shared/components/top-navbar/top-navbar";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Footer, Navbar, TopNavbar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('real-estate-frontend');
}
