import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlogWithSidebar } from './blog-with-sidebar';

describe('BlogWithSidebar', () => {
  let component: BlogWithSidebar;
  let fixture: ComponentFixture<BlogWithSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BlogWithSidebar],
    }).compileComponents();

    fixture = TestBed.createComponent(BlogWithSidebar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
