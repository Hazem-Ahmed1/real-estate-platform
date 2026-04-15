import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeSectionHeaderWithFilters } from './home-section-header-with-filters';

describe('HomeSectionHeaderWithFilters', () => {
  let component: HomeSectionHeaderWithFilters;
  let fixture: ComponentFixture<HomeSectionHeaderWithFilters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeSectionHeaderWithFilters],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeSectionHeaderWithFilters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
