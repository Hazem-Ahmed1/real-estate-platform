import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BlogContent } from './blog-content';

describe('BlogContent', () => {
  let component: BlogContent;
  let fixture: ComponentFixture<BlogContent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BlogContent],
    }).compileComponents();

    fixture = TestBed.createComponent(BlogContent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
