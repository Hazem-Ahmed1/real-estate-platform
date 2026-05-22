import { Injectable } from '@angular/core';
import Swal, { SweetAlertResult } from 'sweetalert2';

@Injectable({ providedIn: 'root' })
export class AlertService {
  /**
   * Show a confirmation dialog with customizable button texts.
   * Returns a promise that resolves to true if confirmed, false otherwise.
   */
  confirm(message: string, confirmText = 'نعم', cancelText = 'لا'): Promise<boolean> {
    return Swal.fire({
      text: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: confirmText,
      cancelButtonText: cancelText,
      reverseButtons: true,
    }).then((result: SweetAlertResult) => !!result.isConfirmed);
  }

  /**
   * Confirmation with an extra "Activate" option for handling inactive items.
   * Resolves to 'activate' if user chooses to activate, otherwise 'cancel'.
   */
  confirmActivate(message: string): Promise<'activate' | 'cancel'> {
    return Swal.fire({
      text: message,
      icon: 'warning',
      showCancelButton: true,
      showDenyButton: true,
      confirmButtonText: 'إلغاء',
      denyButtonText: 'تفعيل',
      cancelButtonText: 'لا',
      reverseButtons: true,
    }).then((result: SweetAlertResult) => {
      if (result.isDenied) return 'activate';
      return 'cancel';
    });
  }
}
