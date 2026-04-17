import { Component } from '@angular/core';
import { ProjectsSection } from "./projects-section/projects-section";
import { StatusSection } from "./status-section/status-section";
import { UnitsSection } from './units-section/units-section';
import { SoldProjectsSection } from './sold-projects-section/sold-projects-section';
import { WhyUsSection } from './why-us-section/why-us-section';
import { Hero } from './hero/hero';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';

@Component({
  selector: 'app-home',
  imports: [Hero, ProjectsSection, UnitsSection, SoldProjectsSection, StatusSection, WhyUsSection, ContactSection],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {}
