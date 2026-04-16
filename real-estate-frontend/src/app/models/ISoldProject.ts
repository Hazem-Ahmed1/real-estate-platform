export interface ISoldProject {
  title: string;
  location: string;
  price: string;
  imageURL: string;
  beds?: number;
  baths?: number;
  lounges?: number;
  area?: string;
  units:number;
  rooms:number;
  streetsText?: string;
  type: 'تم البيع' | 'تم الإيجار',
}
