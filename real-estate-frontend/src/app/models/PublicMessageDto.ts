export interface PublicMessageCreateDto {
  type: string;
  fullName: string;
  email: string;
  phone: string;
  subject: string;
  messageBody: string;
}

export interface PublicMessageCreatedDto {
  messageId: number;
  createdAt: string;
}
