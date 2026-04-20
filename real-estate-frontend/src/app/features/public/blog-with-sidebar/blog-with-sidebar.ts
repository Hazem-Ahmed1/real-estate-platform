import { Component, EventEmitter, Output } from '@angular/core';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { IBlogPost } from '../../../models/IBlogPost';
import { UnitCardModel } from '../../../models/IUnit';
import { BlogPost } from "./blog-post/blog-post";
import { UnitSidebar } from "./unit-sidebar/unit-sidebar";

@Component({
  selector: 'app-blog-with-sidebar',
  imports: [BreadcrumbComponent, ContactSection, BlogPost, UnitSidebar],
  templateUrl: './blog-with-sidebar.html',
  styleUrl: './blog-with-sidebar.css',
})
export class BlogWithSidebar {
  // post: IBlogPost | null = null;
  post: IBlogPost = {
    id: 1,
    title: 'شركة منصات العقارية حصلت جائزة التسويق العقاري الأول لعام 2022',
    date: '24 يوليو 2024',
    image: '/images/imgForFullProject.jpg',
    images: ['/images/p1.jpg', '/images/p2.jpg', '/images/p3.jpg'],
    excerpt: 'نجحت شركة منصات العقارية في تحقيق إنجاز كبير في مجال التسويق العقاري...',
    sections: [
      {
        content: [
          'تُوجت شركة منصات العقارية مسيرتها في مجال التسويق العقاري بتكريم من معالي وزير الشؤون البلدية والقروية والإسكان الأستاذ ماجد بن عبد الله الحقيل بجائزة المسوق العقاري الأول لعام 2022م وكان ذلك في الحدث العقاري الأضخم للمستثمرين والمشترين معرض ريستاتكس العقاري المقام في مركز الرياض للمعارض والمؤتمرات.',
          'ويأتي هذا التكريم سعياً من منصات العقارية لتصبح الرائدة في مجال التسويق العقاري الحديث من خلال المعرفة التامة.'
        ]
      },
      {
        title: 'تصفح آخر عروض العقارات',
        content: [
          'وكما حرصت شركة منصات العقارية منذ انطلاقها بقيادة المؤسس والرئيس التنفيذي الأستاذ خالد المبيض بتقديم أفضل الخدمات العقارية في مجال التسويق العقاري وأيضاً التقييم العقاري المُعتمد وإدارة الأملاك من خلال استخدام التقنيات الحديثة المتطورة والخبرة الكافية بأهمية تطبيق الإستراتيجيات الخاصة بالتسويق العقاري الحديث وأهميته وذلك لنجاح أعمال الشركة والتخطيط لنجاحات أكبر تساهم في تطوير النهضة العقارية في المملكة العربية السعودية.',
          'والجدير بالذكر أن شركة منصات العقارية حصدت العديد من الجوائز كجائزة المسوق العقاري الأول لعام 2021م وجائزة أفضل برنامج تطبيق تسويق عقاري لعام 2018م والعديد من الجوائز في مناسبات عدة.'
        ]
      }
    ]
  };
  // units: UnitCardModel[] = [];
  units: UnitCardModel[] = [
    {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/unit.jpg'
    },
    {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    }, {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    }, {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    }, {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    }, {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    },{
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    }, {
      title: 'فيلا للإيجار حي الملك فيصل',
      location: 'الملك فيصل, الرياض, منطقة الرياض',
      price: '120000 ر.س',
      type: 'فيلا',
      imageURL: '/images/imgForFullProject.jpg'
    },
  ];

  @Output() unitClick = new EventEmitter<UnitCardModel>();

  onUnitClick(unit: UnitCardModel) {
    this.unitClick.emit(unit);
  }
}