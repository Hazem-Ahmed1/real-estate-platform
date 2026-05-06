import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyGoalsSection } from './company-goals-section';

describe('CompanyGoalsSection', () => {
  let component: CompanyGoalsSection;
  let fixture: ComponentFixture<CompanyGoalsSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyGoalsSection],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyGoalsSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
