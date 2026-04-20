import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyVisionText } from './company-vision-text';

describe('CompanyVisionText', () => {
  let component: CompanyVisionText;
  let fixture: ComponentFixture<CompanyVisionText>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyVisionText],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyVisionText);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
