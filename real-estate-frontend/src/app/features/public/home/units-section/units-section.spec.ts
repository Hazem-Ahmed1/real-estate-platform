import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UnitsSection } from './units-section';

describe('UnitsSection', () => {
  let component: UnitsSection;
  let fixture: ComponentFixture<UnitsSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UnitsSection],
    }).compileComponents();

    fixture = TestBed.createComponent(UnitsSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
