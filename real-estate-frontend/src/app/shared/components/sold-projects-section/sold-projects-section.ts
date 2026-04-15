import { Component } from '@angular/core';
import { SoldProjectCard } from "../sold-project-card/sold-project-card";
import { HomeSectionHeaderWithFilters } from "../home-section-header-with-filters/home-section-header-with-filters";

@Component({
  selector: 'app-sold-projects-section',
  imports: [SoldProjectCard, HomeSectionHeaderWithFilters],
  templateUrl: './sold-projects-section.html',
  styleUrl: './sold-projects-section.css',
})
export class SoldProjectsSection {
  projects = [
  {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    units: 7,
    rooms: 4,
    area: '148m²',
    type: 'تم البيع',
    imageURL: 'images/p1.jpg'
  },
    {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    units: 7,
    rooms: 4,
    area: '148m²',
    type: 'تم البيع',
    imageURL: 'images/p1.jpg'
  },
    {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    units: 7,
    rooms: 4,
    area: '148m²',
    type: 'تم البيع',
    imageURL: 'images/p1.jpg'
  }
];

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
}
