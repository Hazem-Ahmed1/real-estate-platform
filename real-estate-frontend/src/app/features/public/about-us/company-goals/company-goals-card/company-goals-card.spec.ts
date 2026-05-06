import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyGoalsCard } from './company-goals-card';

describe('CompanyGoalsCard', () => {
  let component: CompanyGoalsCard;
  let fixture: ComponentFixture<CompanyGoalsCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyGoalsCard],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyGoalsCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
