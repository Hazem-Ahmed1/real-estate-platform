import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyAbout } from './company-about';

describe('CompanyAbout', () => {
  let component: CompanyAbout;
  let fixture: ComponentFixture<CompanyAbout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyAbout],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyAbout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
