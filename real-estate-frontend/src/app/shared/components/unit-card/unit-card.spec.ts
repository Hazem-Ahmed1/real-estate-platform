import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UnitCard } from './unit-card';

describe('UnitCard', () => {
  let component: UnitCard;
  let fixture: ComponentFixture<UnitCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UnitCard],
    }).compileComponents();

    fixture = TestBed.createComponent(UnitCard);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('unit', {
      title: 'مشروع الفيلاج 1',
      location: 'جدة - حي النخبة',
      price: '58.000 ريال',
      beds: 4,
      lounges: 7,
      baths: 2,
      area: '148+148',
      streetsText: 'شارعين',
      type: 'للبيع',
      imageURL: 'images/p1.jpg',
    });

    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
