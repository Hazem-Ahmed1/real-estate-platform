import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WhyUsSection } from './why-us-section';

describe('WhyUsSection', () => {
  let component: WhyUsSection;
  let fixture: ComponentFixture<WhyUsSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WhyUsSection],
    }).compileComponents();

    fixture = TestBed.createComponent(WhyUsSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
