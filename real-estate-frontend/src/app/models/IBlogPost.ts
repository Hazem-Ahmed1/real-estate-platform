export interface IBlogPost {
  id: number;
  title: string;
  date: string;
  image: string;
  images: string[];
  excerpt: string;
  sections: {
    title?: string;
    content: string[]; // multiple paragraphs
  }[];
}