import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SoldProjectsList } from './sold-projects-list';

describe('SoldProjectsList', () => {
  let component: SoldProjectsList;
  let fixture: ComponentFixture<SoldProjectsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoldProjectsList],
    }).compileComponents();

    fixture = TestBed.createComponent(SoldProjectsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
