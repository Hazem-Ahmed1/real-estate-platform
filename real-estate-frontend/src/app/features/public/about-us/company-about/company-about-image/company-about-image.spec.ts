import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyAboutImage } from './company-about-image';

describe('CompanyAboutImage', () => {
  let component: CompanyAboutImage;
  let fixture: ComponentFixture<CompanyAboutImage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyAboutImage],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyAboutImage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
