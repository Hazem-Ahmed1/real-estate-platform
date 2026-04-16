import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SoldProjects } from './sold-projects';

describe('SoldProjects', () => {
  let component: SoldProjects;
  let fixture: ComponentFixture<SoldProjects>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoldProjects],
    }).compileComponents();

    fixture = TestBed.createComponent(SoldProjects);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
