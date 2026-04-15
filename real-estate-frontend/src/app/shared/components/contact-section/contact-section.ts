import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-contact-section',
  imports: [],
  templateUrl: './contact-section.html',
  styleUrl: './contact-section.css',
})
export class ContactSection {
    form = signal({
    firstName: '',
    email: '',
    subject: '',
    message: ''
  });

  updateField(field: string, value: string) {
    this.form.update(f => ({ ...f, [field]: value }));
  }

  submit() {
    console.log(this.form());
  }

}
