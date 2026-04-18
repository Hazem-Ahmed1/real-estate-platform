import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from '../../../shared/components/footer/footer';
import { Navbar } from '../../../shared/components/navbar/navbar';
import { TopNavbar } from '../../../shared/components/top-navbar/top-navbar';

@Component({
  selector: 'app-public-layout',
  imports: [RouterOutlet, TopNavbar, Navbar, Footer],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicLayout {}
