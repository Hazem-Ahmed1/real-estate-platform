export interface PublicBlogImageDto {
  imageId: number;
  imageUrl: string;
}

export interface PublicBlogThumbnailDto {
  imageId: number;
  imageUrl: string;
}

export interface PublicBlogDto {
  blogId: number;
  title: string;
  publishDate: string;
  description?: string;
  thumbnail?: PublicBlogThumbnailDto | null;
  images?: PublicBlogImageDto[];
}
