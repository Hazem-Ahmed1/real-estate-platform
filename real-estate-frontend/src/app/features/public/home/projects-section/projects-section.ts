import { Component } from '@angular/core';
import { ProjectCard } from '../../../../shared/components/project-card/project-card';
import { IProject } from '../../../../models/IProject';
import { HomeSectionHeaderWithFilters } from "../../../../shared/components/home-section-header-with-filters/home-section-header-with-filters";
import { ProjectsList } from "../../../../shared/components/projects-list/projects-list";

@Component({
  selector: 'app-projects-section',
  imports: [HomeSectionHeaderWithFilters, ProjectsList],
  templateUrl: './projects-section.html',
  styleUrl: './projects-section.css',
})
export class ProjectsSection {

  filter(val:string){
    if(val == 'rent'){
      console.log("rent");
    }
    else if(val == "sell"){
      console.log("sell");
    }
    else{
      console.log("all");
    }
  }
  projects: IProject[] = [
    {
      title: 'مشروع الفيلاج 1',
      location: 'جدة - حي النخيل',
      type: 'للبيع',
      imageUrl: 'images/p1.jpg',
    },
    {
      title: 'مشروع الفيلاج 2',
      location: 'جدة - حي النخيل',
      type: 'للبيع',
      imageUrl: 'images/p2.jpg',
    },
    {
      title: 'مشروع الفيلاج 3',
      location: 'جدة - حي النخيل',
      type: 'للإيجار',
      imageUrl: 'images/p3.jpg',
    },
    {
      title: 'مشروع الفيلاج 1',
      location: 'جدة - حي النخيل',
      type: 'للبيع',
      imageUrl: 'images/p1.jpg',
    },
    {
      title: 'مشروع الفيلاج 2',
      location: 'جدة - حي النخيل',
      type: 'للبيع',
      imageUrl: 'images/p2.jpg',
    },
    {
      title: 'مشروع الفيلاج 3',
      location: 'جدة - حي النخيل',
      type: 'للإيجار',
      imageUrl: 'images/p3.jpg',
    }
  ];
}
