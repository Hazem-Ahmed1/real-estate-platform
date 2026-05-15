export interface AdminBlogImageDto {
  imageId: number;
  imageUrl: string;
}

export interface AdminBlogThumbnailDto {
  imageId: number;
  imageUrl: string;
}

export interface AdminBlogDto {
  blogId: number;
  title: string;
  publishDate: string;
  description?: string;
  thumbnail?: AdminBlogThumbnailDto | null;
  images?: AdminBlogImageDto[];
}
