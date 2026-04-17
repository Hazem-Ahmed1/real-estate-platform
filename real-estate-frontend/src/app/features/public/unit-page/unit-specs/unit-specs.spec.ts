import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UnitSpecs } from './unit-specs';

describe('UnitSpecs', () => {
  let component: UnitSpecs;
  let fixture: ComponentFixture<UnitSpecs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UnitSpecs],
    }).compileComponents();

    fixture = TestBed.createComponent(UnitSpecs);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
