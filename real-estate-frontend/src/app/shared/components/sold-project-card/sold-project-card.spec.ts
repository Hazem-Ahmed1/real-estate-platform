import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SoldProjectCard } from './sold-project-card';

describe('SoldProjectCard', () => {
  let component: SoldProjectCard;
  let fixture: ComponentFixture<SoldProjectCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoldProjectCard],
    }).compileComponents();

    fixture = TestBed.createComponent(SoldProjectCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
