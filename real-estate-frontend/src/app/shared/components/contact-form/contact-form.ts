import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-contact-form',
  imports: [],
  templateUrl: './contact-form.html',
  styleUrl: './contact-form.css',
})
export class ContactForm {
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
