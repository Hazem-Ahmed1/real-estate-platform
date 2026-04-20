import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WhyUsCard } from './why-us-card';

describe('WhyUsCard', () => {
  let component: WhyUsCard;
  let fixture: ComponentFixture<WhyUsCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WhyUsCard],
    }).compileComponents();

    fixture = TestBed.createComponent(WhyUsCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
