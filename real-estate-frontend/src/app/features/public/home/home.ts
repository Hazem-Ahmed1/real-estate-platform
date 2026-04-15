import { Component } from '@angular/core';
import { Hero } from "../../../shared/components/hero/hero";
import { ProjectsSection } from "../../../shared/components/projects-section/projects-section";
import { UnitsSection } from "../../../shared/components/units-section/units-section";
import { SoldProjectsSection } from "../../../shared/components/sold-projects-section/sold-projects-section";
import { StatusCard } from "../../../shared/components/status-card/status-card";
import { StatusSection } from "../../../shared/components/status-section/status-section";
import { WhyUsSection } from "../../../shared/components/why-us-section/why-us-section";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";

@Component({
  selector: 'app-home',
  imports: [Hero, ProjectsSection, UnitsSection, SoldProjectsSection, StatusCard, StatusSection, WhyUsSection, ContactSection],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {}
