export interface ISoldProject {
  id?: number;
  title: string;
  location: string;
  price: string;
  imageURL: string;
  beds?: number;
  baths?: number;
  lounges?: number;
  area?: string;
  units: number;
  buildings?: number;
  rooms: number;
  streetsText?: string;
  type: 'تم البيع' | 'تم الإيجار',
}
