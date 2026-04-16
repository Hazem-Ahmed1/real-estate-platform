import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusSection } from './status-section';

describe('StatusSection', () => {
  let component: StatusSection;
  let fixture: ComponentFixture<StatusSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatusSection],
    }).compileComponents();

    fixture = TestBed.createComponent(StatusSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
