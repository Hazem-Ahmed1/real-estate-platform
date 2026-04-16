import { Component } from '@angular/core';
import { HomeSectionHeaderWithFilters } from '../../../../shared/components/home-section-header-with-filters/home-section-header-with-filters';
import { UnitCard } from '../../../../shared/components/unit-card/unit-card';

@Component({
  selector: 'app-units-section',
  imports: [UnitCard, HomeSectionHeaderWithFilters],
  templateUrl: './units-section.html',
  styleUrl: './units-section.css',
})
export class UnitsSection {
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
  units = [
  {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    price: '58.000 ريال',
    beds: 4,
    lounges: 7,
    baths: 2,
    area: '148+148',
    streetsText: 'شارعين',
    type: 'للبيع',
    imageURL: 'images/p1.jpg'
  },
    {
    title: 'مشروع الفيلاج 1',
    location: 'جدة - حي النخبة',
    price: '58.000 ريال',
    beds: 4,
    lounges: 7,
    baths: 2,
    area: '148+148',
    streetsText: 'شارعين',
    type: 'للبيع',
    imageURL: 'images/p1.jpg'
  },
  // duplicate for demo
];
}
