import { Component, input, signal } from '@angular/core';
import { ProjectCard } from "../project-card/project-card";
import { SoldProjectCard } from "../sold-project-card/sold-project-card";
import { IProject } from '../../../models/IProject';
import { ISoldProject } from '../../../models/ISoldProject';

@Component({
  selector: 'app-projects-list',
  imports: [ProjectCard, SoldProjectCard],
  templateUrl: './projects-list.html',
  styleUrl: './projects-list.css',
})
export class ProjectsList {
  projects = input.required<IProject[]>();

  mapToSoldProject(p: IProject): ISoldProject {
    return {
      id: p.projectId,
      title: p.name,
      location: [p.city, p.region].filter(Boolean).join(' - '),
      price: 'اتصل بنا',
      imageURL: p.thumbnailUrl || 'images/p1.jpg',
      area: p.landArea ? `${p.landArea} * ${p.landArea}` : undefined,
      units: p.unitsNumber,
      buildings: p.buildingsNumber,
      rooms: p.totalRooms,
      type: p.status === 'Sold' ? 'تم البيع' : 'تم الإيجار'
    };
  }
}
