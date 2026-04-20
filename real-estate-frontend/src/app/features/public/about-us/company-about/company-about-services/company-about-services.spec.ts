import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyAboutServices } from './company-about-services';

describe('CompanyAboutServices', () => {
  let component: CompanyAboutServices;
  let fixture: ComponentFixture<CompanyAboutServices>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyAboutServices],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyAboutServices);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
