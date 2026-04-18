import { Component } from '@angular/core';
import { TopNavbar } from "../../../shared/components/top-navbar/top-navbar";
import { Navbar } from "../../../shared/components/navbar/navbar";
import { ContactSection } from "../../../shared/components/contact-section/contact-section";
import { Footer } from "../../../shared/components/footer/footer";
import { BlogContent } from "./blog-content/blog-content";

@Component({
  selector: 'app-blog',
  imports: [TopNavbar, Navbar, ContactSection, Footer, BlogContent],
  templateUrl: './blog.html',
  styleUrl: './blog.css',
})
export class Blog {}
