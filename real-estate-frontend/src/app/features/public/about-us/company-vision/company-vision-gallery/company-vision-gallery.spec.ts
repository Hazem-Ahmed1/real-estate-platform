import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyVisionGallery } from './company-vision-gallery';

describe('CompanyVisionGallery', () => {
  let component: CompanyVisionGallery;
  let fixture: ComponentFixture<CompanyVisionGallery>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyVisionGallery],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyVisionGallery);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
