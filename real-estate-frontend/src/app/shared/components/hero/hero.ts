import { Component } from '@angular/core';
import { SearchFilter } from "../search-filter/search-filter";

@Component({
  selector: 'app-hero',
  imports: [SearchFilter],
  templateUrl: './hero.html',
  styleUrl: './hero.css',
})
export class Hero {
  
}
