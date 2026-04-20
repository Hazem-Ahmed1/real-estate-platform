import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyVision } from './company-vision';

describe('CompanyVision', () => {
  let component: CompanyVision;
  let fixture: ComponentFixture<CompanyVision>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanyVision],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanyVision);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
