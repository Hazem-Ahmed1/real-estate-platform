import { Component, inject } from '@angular/core';
import { ProjectsSection } from "./projects-section/projects-section";
import { StatusSection } from "./status-section/status-section";
import { UnitsSection } from './units-section/units-section';
import { SoldProjectsSection } from './sold-projects-section/sold-projects-section';
import { WhyUsSection } from './why-us-section/why-us-section';
import { Hero } from './hero/hero';
import { ContactSection } from '../../../shared/components/contact-section/contact-section';
import { SearchStateService } from '../../../services/search-state.service';
import { ProjectsList } from '../../../shared/components/projects-list/projects-list';
import { UnitsList } from '../../../shared/components/units-list/units-list';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';

@Component({
  selector: 'app-home',
  imports: [
    Hero,
    ProjectsSection,
    UnitsSection,
    SoldProjectsSection,
    StatusSection,
    WhyUsSection,
    ContactSection,
    ProjectsList,
    UnitsList,
    PaginationComponent
  ],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  readonly searchState = inject(SearchStateService);

  clearSearch(): void {
    this.searchState.reset();
  }

  onPageChange(page: number): void {
    this.searchState.search(
      this.searchState.lastParams(),
      this.searchState.searchType(),
      page
    );
  }
}
