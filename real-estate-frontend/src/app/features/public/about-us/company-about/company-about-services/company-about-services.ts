import { Component } from '@angular/core';
import { CommonModule, NgFor } from '@angular/common';

interface Service {
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'company-about-services',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './company-about-services.html',
  styleUrl: './company-about-services.css'
})
export class CompanyAboutServices {
  services: Service[] = [
    {
      title: 'شراء عقارات',
      description: 'تصفح ملايين العقارات في مدينتك، وحذف العقارات المفضلة لديك وإعداد تنبيهات البحث',
      icon: 'M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z M9 22V12h6v10'
    },
    {
      title: 'بيـع عقارات',
      description: 'تصفح ملايين العقارات في مدينتك، وحذف العقارات المفضلة لديك وإعداد تنبيهات البحث',
      icon: 'M12 2a10 10 0 1 0 0 20A10 10 0 0 0 12 2zm0 4v8m-4-4h8'
    },
    {
      title: 'إيجار شقق ومباني سكنية',
      description: 'تصفح ملايين العقارات في مدينتك، وحذف العقارات المفضلة لديك وإعداد تنبيهات البحث',
      icon: 'M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z M12 7a3 3 0 1 0 0 6 3 3 0 0 0 0-6z'
    }
  ];
}